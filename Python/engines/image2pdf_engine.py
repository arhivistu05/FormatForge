"""
Image to PDF Engine
Folosește Pillow (PIL) pentru conversie
Suportă: JPG, PNG, BMP, GIF, TIFF, WEBP
"""

from pathlib import Path
import sys
from PIL import Image
import os

# ============================================================
# FORMATURI SUPORTATE
# ============================================================

RASTER_IMAGE_FORMATS = {
    ".jpg": "JPEG Image",
    ".jpeg": "JPEG Image",
    ".png": "PNG Image",
    ".bmp": "Bitmap Image",
    ".gif": "GIF Image",
    ".tiff": "TIFF Image",
    ".tif": "TIFF Image",
    ".webp": "WebP Image",
    ".ico": "ICO Image",
}

IMAGE_FORMATS = {**RASTER_IMAGE_FORMATS}


# ============================================================
# DETECȚIE FORMAT
# ============================================================

def get_file_extension(file_path):
    """Get the file extension from the file path."""
    return Path(file_path).suffix.lower()


def is_raster_image_format(file_path):
    """Check if the file is a raster image."""
    return get_file_extension(file_path) in RASTER_IMAGE_FORMATS


def is_image_format(file_path):
    """Check if the file is a supported image."""
    return get_file_extension(file_path) in IMAGE_FORMATS


def get_format_type(file_path):
    """Get the format type of the file."""
    extension = get_file_extension(file_path)
    return IMAGE_FORMATS.get(extension, None)


# ============================================================
# VALIDARE
# ============================================================

def validate_input_file(file_path):
    """Validate the input image."""
    input_file = Path(file_path)

    if not input_file.exists():
        raise FileNotFoundError(f"Input file '{file_path}' does not exist.")

    if not input_file.is_file():
        raise ValueError(f"Input path '{file_path}' is not a file.")

    if not is_image_format(file_path):
        raise ValueError(
            f"Input file '{file_path}' is not a supported image format.\n"
            f"Supported formats: {', '.join(IMAGE_FORMATS.keys())}"
        )

    return input_file


def get_output_file(input_file, output_file=None):
    """Get the output PDF file path."""
    input_path = Path(input_file)

    if output_file is None:
        output_path = input_path.with_suffix(".pdf")
    else:
        output_path = Path(output_file)

    output_path.parent.mkdir(parents=True, exist_ok=True)

    return output_path


# ============================================================
# CONVERSIE PRINCIPALĂ
# ============================================================

def convert_to_pdf(input_file, output_file=None, progress_callback=None):
    """
    Convert an image to PDF.
    
    Args:
        input_file: Path to the input image
        output_file: Path to the output PDF (optional)
        progress_callback: Function to call with progress (0.0 - 1.0)
    
    Returns:
        Path to the output PDF file
    """
    if progress_callback:
        progress_callback(0.1)
    
    input_path = validate_input_file(input_file)
    output_path = get_output_file(input_file, output_file)
    
    if progress_callback:
        progress_callback(0.2)
    
    print(f"\n🖼️  Converting: {input_path.name}")
    print(f"   Type: {get_format_type(str(input_path))}")
    print(f"   Output: {output_path}")
    print(f"   Using: Pillow")
    
    if progress_callback:
        progress_callback(0.3)
    
    try:
        # Deschide imaginea
        image = Image.open(input_path)
        
        if progress_callback:
            progress_callback(0.5)
        
        # Obține dimensiunile
        width, height = image.size
        print(f"   Dimensions: {width}x{height}")
        print(f"   Mode: {image.mode}")
        
        # Convertește la RGB dacă este necesar
        if image.mode in ("RGBA", "LA", "P"):
            print("   Converting mode: RGBA/LA/P -> RGB")
            image = image.convert("RGB")
        elif image.mode != "RGB":
            print(f"   Converting mode: {image.mode} -> RGB")
            image = image.convert("RGB")
        
        if progress_callback:
            progress_callback(0.7)
        
        # Salvează ca PDF
        image.save(
            output_path,
            "PDF",
            resolution=100.0
        )
        
        if progress_callback:
            progress_callback(0.9)
        
        image.close()
        
        # Verifică rezultatul
        if not output_path.exists():
            raise RuntimeError(f"Output file not created: {output_path}")
        
        if output_path.stat().st_size == 0:
            raise RuntimeError(f"Output file is empty: {output_path}")
        
        size_mb = output_path.stat().st_size / (1024 * 1024)
        print(f"   ✅ Success! Size: {size_mb:.2f} MB")
        print(f"   📁 {output_path}")
        
        if progress_callback:
            progress_callback(1.0)
        
        return output_path
        
    except Exception as error:
        error_msg = f"Failed to convert '{input_path.name}' to PDF: {error}"
        print(f"   ❌ Error: {error_msg}")
        raise RuntimeError(error_msg) from error


def batch_convert_to_pdf(input_files, output_folder=None, progress_callback=None):
    """
    Convert multiple images to PDF.
    
    Args:
        input_files: List of input file paths
        output_folder: Output folder path (optional)
        progress_callback: Function to call with progress (0.0 - 1.0)
    
    Returns:
        List of output PDF paths
    """
    if not input_files:
        return []
    
    results = []
    total = len(input_files)
    
    for i, input_file in enumerate(input_files):
        if progress_callback:
            progress = (i / total) * 0.9
            progress_callback(progress)
        
        print(f"\n[{i+1}/{total}] Processing...")
        
        if output_folder:
            input_path = Path(input_file)
            output_file = Path(output_folder) / (input_path.stem + ".pdf")
            result = convert_to_pdf(input_file, str(output_file), progress_callback)
        else:
            result = convert_to_pdf(input_file, None, progress_callback)
        
        results.append(result)
    
    if progress_callback:
        progress_callback(1.0)
    
    return results


def merge_images_to_pdf(image_files, output_file=None, progress_callback=None):
    """
    Merge multiple images into a single PDF.
    
    Args:
        image_files: List of image file paths
        output_file: Path to the output PDF (optional)
        progress_callback: Function to call with progress (0.0 - 1.0)
    
    Returns:
        Path to the output PDF file
    """
    if not image_files:
        raise ValueError("No image files provided")
    
    if progress_callback:
        progress_callback(0.1)
    
    # Validează toate fișierele
    images = []
    for img_file in image_files:
        validate_input_file(img_file)
    
    if progress_callback:
        progress_callback(0.2)
    
    # Determină output
    if output_file is None:
        input_path = Path(image_files[0])
        output_path = input_path.with_suffix(".pdf")
    else:
        output_path = Path(output_file)
    
    output_path.parent.mkdir(parents=True, exist_ok=True)
    
    print(f"\n🖼️  Merging {len(image_files)} images to PDF")
    print(f"   Output: {output_path}")
    print(f"   Using: Pillow")
    
    if progress_callback:
        progress_callback(0.3)
    
    try:
        # Deschide toate imaginile
        pil_images = []
        for i, img_file in enumerate(image_files):
            if progress_callback:
                progress = 0.3 + (i / len(image_files)) * 0.4
                progress_callback(progress)
            
            img = Image.open(img_file)
            
            # Convertește la RGB
            if img.mode in ("RGBA", "LA", "P"):
                img = img.convert("RGB")
            elif img.mode != "RGB":
                img = img.convert("RGB")
            
            pil_images.append(img)
        
        if progress_callback:
            progress_callback(0.8)
        
        # Salvează ca PDF (prima imagine + restul ca pagini)
        if pil_images:
            pil_images[0].save(
                output_path,
                "PDF",
                resolution=100.0,
                save_all=True,
                append_images=pil_images[1:]
            )
        
        # Închide toate imaginile
        for img in pil_images:
            img.close()
        
        if progress_callback:
            progress_callback(0.9)
        
        # Verifică rezultatul
        if not output_path.exists():
            raise RuntimeError(f"Output file not created: {output_path}")
        
        if output_path.stat().st_size == 0:
            raise RuntimeError(f"Output file is empty: {output_path}")
        
        size_mb = output_path.stat().st_size / (1024 * 1024)
        print(f"   ✅ Success! Size: {size_mb:.2f} MB")
        print(f"   📁 {output_path}")
        
        if progress_callback:
            progress_callback(1.0)
        
        return output_path
        
    except Exception as error:
        error_msg = f"Failed to merge images to PDF: {error}"
        print(f"   ❌ Error: {error_msg}")
        raise RuntimeError(error_msg) from error


# ============================================================
# INFORMAȚII
# ============================================================

def get_engine_name():
    return "Pillow"


def supported_formats():
    return list(IMAGE_FORMATS.keys())


def get_supported_raster_formats():
    return sorted(RASTER_IMAGE_FORMATS.keys())


def is_pillow_installed():
    try:
        import PIL
        return True
    except ImportError:
        return False


# ============================================================
# TESTARE
# ============================================================

if __name__ == "__main__":
    print("=" * 50)
    print("Image to PDF Engine")
    print("=" * 50)
    print(f"Pillow installed: {'✅' if is_pillow_installed() else '❌'}")
    print(f"Supported formats:")
    for fmt in supported_formats():
        print(f"  {fmt}")
    print("=" * 50)