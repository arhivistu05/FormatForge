"""
Office to PDF Converter - Module Component
Fără interfață grafică - doar funcții pentru conversie
"""

from pathlib import Path
import sys
import runtime_bootstrap

runtime_bootstrap.configure()

from engines import office2pdf_engine


# ============================================================
# FUNCȚII PRINCIPALE
# ============================================================

def convert_office_to_pdf(input_file, output_file=None, progress_callback=None):
    """
    Convert an Office file to PDF.
    
    Args:
        input_file: Path to the input Office file (.docx, .xlsx, .pptx, etc.)
        output_file: Path to the output PDF file (optional)
        progress_callback: Function to call with progress updates (0.0 - 1.0)
    
    Returns:
        Path to the output PDF file
    
    Raises:
        FileNotFoundError: If input file doesn't exist
        ValueError: If input file is not a supported Office format
        RuntimeError: If conversion fails
    """
    return office2pdf_engine.convert_to_pdf(input_file, output_file, progress_callback)


def batch_convert_office_to_pdf(input_files, output_folder=None, progress_callback=None):
    """
    Convert multiple Office files to PDF.
    
    Args:
        input_files: List of input file paths
        output_folder: Output folder path (optional)
        progress_callback: Function to call with progress updates (0.0 - 1.0)
    
    Returns:
        List of output PDF paths
    """
    return office2pdf_engine.batch_convert_to_pdf(input_files, output_folder, progress_callback)


# ============================================================
# FUNCȚII DE UTILITATE
# ============================================================

def get_supported_office_formats():
    """Return all supported Office formats."""
    return office2pdf_engine.supported_formats()


def get_format_type(file_path):
    """Get the format type of the file (Word, Excel, PowerPoint)."""
    return office2pdf_engine.get_format_type(file_path)


def is_office_file(file_path):
    """Check if the file is a supported Office file."""
    return office2pdf_engine.is_office_format(file_path)


def get_engine_info():
    """Get information about the conversion engine."""
    return office2pdf_engine.get_engine_info()


# ============================================================
# FUNCȚII PENTRU LINIA DE COMANDĂ
# ============================================================

def main():
    """Main entry point for command line usage."""
    import argparse
    
    parser = argparse.ArgumentParser(
        description="Convert Microsoft Office files to PDF",
        epilog="Supported formats: .doc, .docx, .xls, .xlsx, .ppt, .pptx"
    )
    parser.add_argument(
        "input",
        nargs="?",
        help="Input file path"
    )
    parser.add_argument(
        "-o", "--output",
        help="Output PDF file path (optional)"
    )
    parser.add_argument(
        "--info",
        action="store_true",
        help="Show supported formats and engine info"
    )
    parser.add_argument(
        "--batch",
        nargs="+",
        help="Multiple input files for batch conversion"
    )
    parser.add_argument(
        "--output-folder",
        help="Output folder for batch conversion"
    )
    
    args = parser.parse_args()
    
    # Afișează informații
    if args.info:
        info = get_engine_info()
        print("=" * 50)
        print("FormatForge - Office Converter")
        print("=" * 50)
        print(f"Engine: {info.get('name', 'office2pdf_engine')}")
        print(f"Version: {info.get('version', '1.0.0')}")
        print(f"Supported formats:")
        for fmt in get_supported_office_formats():
            print(f"  {fmt}")
        print("=" * 50)
        return
    
    # Batch conversion
    if args.batch:
        if not args.output_folder:
            print("Error: --output-folder is required for batch conversion")
            sys.exit(1)
        
        results = batch_convert_office_to_pdf(args.batch, args.output_folder)
        success = [r for r in results if r is not None]
        failed = len(results) - len(success)
        
        print(f"\n✅ Success: {len(success)}")
        print(f"❌ Failed: {failed}")
        return
    
    # Single file conversion
    if not args.input:
        parser.print_help()
        return
    
    if not Path(args.input).exists():
        print(f"Error: Input file not found: {args.input}")
        sys.exit(1)
    
    try:
        result = convert_office_to_pdf(args.input, args.output)
        print(f"✅ Success! Output: {result}")
    except Exception as e:
        print(f"❌ Error: {e}")
        sys.exit(1)


if __name__ == "__main__":
    main()
