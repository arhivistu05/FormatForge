using System;
using System.Buffers.Binary;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;

namespace FormatForge.App;

internal static class PreviewImageLoader
{
    private const int MaxEmbeddedImageBytes = 25 * 1024 * 1024;

    public static bool TryLoadPreview(string filePath, string category, bool includeAudioArtwork, Size targetSize, out Image image)
    {
        image = null!;
        if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
        {
            return false;
        }

        try
        {
            if (category == "Images")
            {
                using FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
                using Image source = Image.FromStream(stream, useEmbeddedColorManagement: true, validateImageData: false);
                image = CreatePreviewBitmap(source, targetSize);
                return true;
            }

            if (category == "Audio" && includeAudioArtwork)
            {
                byte[]? artwork = TryReadMp3Artwork(filePath) ?? TryReadFlacArtwork(filePath);
                if (artwork == null || artwork.Length == 0)
                {
                    return false;
                }

                using MemoryStream memory = new MemoryStream(artwork, writable: false);
                using Image source = Image.FromStream(memory, useEmbeddedColorManagement: true, validateImageData: false);
                image = CreatePreviewBitmap(source, targetSize);
                return true;
            }
        }
        catch
        {
        }

        return false;
    }

    private static Bitmap CreatePreviewBitmap(Image source, Size targetSize)
    {
        int maxWidth = Math.Max(96, targetSize.Width);
        int maxHeight = Math.Max(96, targetSize.Height);
        double scale = Math.Min((double)maxWidth / source.Width, (double)maxHeight / source.Height);
        scale = Math.Min(1.0, Math.Max(0.05, scale));
        int width = Math.Max(1, (int)Math.Round(source.Width * scale));
        int height = Math.Max(1, (int)Math.Round(source.Height * scale));

        Bitmap bitmap = new Bitmap(width, height);
        using Graphics graphics = Graphics.FromImage(bitmap);
        graphics.CompositingQuality = CompositingQuality.HighQuality;
        graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
        graphics.SmoothingMode = SmoothingMode.HighQuality;
        graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
        graphics.DrawImage(source, new Rectangle(0, 0, width, height));
        return bitmap;
    }

    private static byte[]? TryReadMp3Artwork(string filePath)
    {
        if (!Path.GetExtension(filePath).Equals(".mp3", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        using FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
        Span<byte> header = stackalloc byte[10];
        if (stream.Read(header) != 10 || header[0] != (byte)'I' || header[1] != (byte)'D' || header[2] != (byte)'3')
        {
            return null;
        }

        int major = header[3];
        int tagSize = DecodeSyncSafe(header.Slice(6, 4));
        if (tagSize <= 0)
        {
            return null;
        }

        long tagEnd = Math.Min(stream.Length, 10L + tagSize);
        if (major == 2)
        {
            return TryReadId3V22Artwork(stream, tagEnd);
        }

        while (stream.Position + 10 <= tagEnd)
        {
            byte[] frameHeader = new byte[10];
            if (stream.Read(frameHeader, 0, frameHeader.Length) != frameHeader.Length || frameHeader.All(value => value == 0))
            {
                break;
            }

            string id = Encoding.ASCII.GetString(frameHeader, 0, 4);
            int frameSize = major == 4 ? DecodeSyncSafe(frameHeader.AsSpan(4, 4)) : BinaryPrimitives.ReadInt32BigEndian(frameHeader.AsSpan(4, 4));
            if (frameSize <= 0 || stream.Position + frameSize > tagEnd)
            {
                break;
            }

            if (id == "APIC" && frameSize <= MaxEmbeddedImageBytes)
            {
                byte[] frame = new byte[frameSize];
                if (stream.Read(frame, 0, frame.Length) != frame.Length)
                {
                    return null;
                }

                return ExtractApicImage(frame);
            }

            stream.Seek(frameSize, SeekOrigin.Current);
        }

        return null;
    }

    private static byte[]? TryReadId3V22Artwork(Stream stream, long tagEnd)
    {
        while (stream.Position + 6 <= tagEnd)
        {
            byte[] frameHeader = new byte[6];
            if (stream.Read(frameHeader, 0, frameHeader.Length) != frameHeader.Length || frameHeader.All(value => value == 0))
            {
                break;
            }

            string id = Encoding.ASCII.GetString(frameHeader, 0, 3);
            int frameSize = (frameHeader[3] << 16) | (frameHeader[4] << 8) | frameHeader[5];
            if (frameSize <= 0 || stream.Position + frameSize > tagEnd)
            {
                break;
            }

            if (id == "PIC" && frameSize <= MaxEmbeddedImageBytes)
            {
                byte[] frame = new byte[frameSize];
                if (stream.Read(frame, 0, frame.Length) != frame.Length)
                {
                    return null;
                }

                return ExtractPicImage(frame);
            }

            stream.Seek(frameSize, SeekOrigin.Current);
        }

        return null;
    }

    private static byte[]? ExtractApicImage(byte[] frame)
    {
        if (frame.Length < 5)
        {
            return null;
        }

        int index = 1;
        int mimeEnd = Array.IndexOf(frame, (byte)0, index);
        if (mimeEnd < 0)
        {
            return null;
        }

        index = mimeEnd + 1;
        if (index >= frame.Length)
        {
            return null;
        }

        index++;
        index = SkipEncodedText(frame, index, frame[0]);
        if (index < 0 || index >= frame.Length)
        {
            return null;
        }

        return frame.Skip(index).ToArray();
    }

    private static byte[]? ExtractPicImage(byte[] frame)
    {
        if (frame.Length < 6)
        {
            return null;
        }

        int index = 1 + 3 + 1;
        index = SkipEncodedText(frame, index, frame[0]);
        if (index < 0 || index >= frame.Length)
        {
            return null;
        }

        return frame.Skip(index).ToArray();
    }

    private static int SkipEncodedText(byte[] frame, int index, byte encoding)
    {
        if (encoding == 1 || encoding == 2)
        {
            for (int i = index; i + 1 < frame.Length; i += 2)
            {
                if (frame[i] == 0 && frame[i + 1] == 0)
                {
                    return i + 2;
                }
            }

            return -1;
        }

        int end = Array.IndexOf(frame, (byte)0, index);
        return end < 0 ? -1 : end + 1;
    }

    private static byte[]? TryReadFlacArtwork(string filePath)
    {
        if (!Path.GetExtension(filePath).Equals(".flac", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        using FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
        Span<byte> marker = stackalloc byte[4];
        if (stream.Read(marker) != 4 || marker[0] != (byte)'f' || marker[1] != (byte)'L' || marker[2] != (byte)'a' || marker[3] != (byte)'C')
        {
            return null;
        }

        bool isLast = false;
        while (!isLast && stream.Position + 4 <= stream.Length)
        {
            Span<byte> header = stackalloc byte[4];
            if (stream.Read(header) != 4)
            {
                return null;
            }

            isLast = (header[0] & 0x80) != 0;
            int blockType = header[0] & 0x7F;
            int blockLength = (header[1] << 16) | (header[2] << 8) | header[3];
            if (blockLength < 0 || stream.Position + blockLength > stream.Length)
            {
                return null;
            }

            if (blockType != 6)
            {
                stream.Seek(blockLength, SeekOrigin.Current);
                continue;
            }

            if (blockLength > MaxEmbeddedImageBytes + 1024)
            {
                return null;
            }

            byte[] block = new byte[blockLength];
            if (stream.Read(block, 0, block.Length) != block.Length)
            {
                return null;
            }

            return ExtractFlacPicture(block);
        }

        return null;
    }

    private static byte[]? ExtractFlacPicture(byte[] block)
    {
        int index = 0;
        if (!TryReadUInt32(block, ref index, out _)) return null;
        if (!TryReadUInt32(block, ref index, out uint mimeLength)) return null;
        if (!Skip(block, ref index, mimeLength)) return null;
        if (!TryReadUInt32(block, ref index, out uint descriptionLength)) return null;
        if (!Skip(block, ref index, descriptionLength)) return null;
        if (!Skip(block, ref index, 16)) return null;
        if (!TryReadUInt32(block, ref index, out uint dataLength)) return null;
        if (dataLength == 0 || dataLength > MaxEmbeddedImageBytes || dataLength > int.MaxValue) return null;
        int length = (int)dataLength;
        if (index > block.Length - length) return null;

        byte[] data = new byte[length];
        Buffer.BlockCopy(block, index, data, 0, data.Length);
        return data;
    }

    private static bool TryReadUInt32(byte[] data, ref int index, out uint value)
    {
        value = 0;
        if (index + 4 > data.Length)
        {
            return false;
        }

        value = BinaryPrimitives.ReadUInt32BigEndian(data.AsSpan(index, 4));
        index += 4;
        return true;
    }

    private static bool Skip(byte[] data, ref int index, uint count)
    {
        if (count > int.MaxValue)
        {
            return false;
        }

        int length = (int)count;
        if (index > data.Length - length)
        {
            return false;
        }

        index += length;
        return true;
    }

    private static int DecodeSyncSafe(ReadOnlySpan<byte> bytes)
    {
        return (bytes[0] << 21) | (bytes[1] << 14) | (bytes[2] << 7) | bytes[3];
    }
}
