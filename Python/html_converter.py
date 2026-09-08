"""
HTML Converter - HTML/HTM to PDF
Fără interfață grafică - doar funcții pentru conversie
Folosește Playwright (Chromium) pentru conversie
"""

from pathlib import Path
import sys
import runtime_bootstrap

runtime_bootstrap.configure()

from engines import html_to_pdf_engine


# ============================================================
# FUNCȚII PRINCIPALE
# ============================================================

def convert_html_to_pdf(input_file, output_file=None, progress_callback=None):
    """
    Convert an HTML/HTM file to PDF.
    
    Args:
        input_file: Path to the input HTML file (.html, .htm)
        output_file: Path to the output PDF file (optional)
        progress_callback: Function to call with progress updates (0.0 - 1.0)
    
    Returns:
        Path to the output PDF file
    
    Raises:
        FileNotFoundError: If input file doesn't exist
        ValueError: If input file is not a supported HTML format
        RuntimeError: If conversion fails
    """
    return html_to_pdf_engine.convert_html_to_pdf(input_file, output_file, progress_callback)


def batch_convert_html_to_pdf(input_files, output_folder=None, progress_callback=None):
    """
    Convert multiple HTML files to PDF.
    
    Args:
        input_files: List of input file paths
        output_folder: Output folder path (optional)
        progress_callback: Function to call with progress updates (0.0 - 1.0)
    
    Returns:
        List of output PDF paths
    """
    return html_to_pdf_engine.batch_convert_html_to_pdf(input_files, output_folder, progress_callback)


# ============================================================
# FUNCȚII DE UTILITATE
# ============================================================

def get_supported_formats():
    """Return all supported HTML formats."""
    return html_to_pdf_engine.supported_formats()


def is_html_file(file_path):
    """Check if the file is a supported HTML file."""
    return html_to_pdf_engine.is_html_or_htm_format(file_path)


def get_format_type(file_path):
    """Get the format type of the file."""
    return html_to_pdf_engine.get_format_type(file_path)


def get_engine_info():
    """Get information about the conversion engine."""
    return {
        "name": html_to_pdf_engine.get_engine_name(),
        "supported_formats": html_to_pdf_engine.supported_formats(),
        "playwright_installed": html_to_pdf_engine.is_playwright_installed()
    }


# ============================================================
# FUNCȚII PENTRU LINIA DE COMANDĂ
# ============================================================

def main():
    """Main entry point for command line usage."""
    import argparse
    
    parser = argparse.ArgumentParser(
        description="Convert HTML/HTM files to PDF using Playwright (Chromium)",
        epilog="Supported formats: .html, .htm"
    )
    parser.add_argument(
        "input",
        nargs="?",
        help="Input HTML file path"
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
        print("FormatForge - HTML to PDF Converter")
        print("=" * 50)
        print(f"Engine: {info['name']}")
        print(f"Playwright installed: {'✅' if info['playwright_installed'] else '❌'}")
        print(f"Supported formats:")
        for fmt in info['supported_formats']:
            print(f"  {fmt}")
        print("=" * 50)
        
        if not info['playwright_installed']:
            print("\n⚠️  Playwright not installed!")
            print("   Run the following commands:")
            print("   pip install playwright")
            print("   python -m playwright install")
        
        return
    
    # Batch conversion
    if args.batch:
        if not args.output_folder:
            print("Error: --output-folder is required for batch conversion")
            sys.exit(1)
        
        results = batch_convert_html_to_pdf(args.batch, args.output_folder)
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
        result = convert_html_to_pdf(args.input, args.output)
        print(f"✅ Success! Output: {result}")
    except Exception as e:
        print(f"❌ Error: {e}")
        sys.exit(1)


if __name__ == "__main__":
    main()
