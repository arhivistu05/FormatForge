"""
ReportLab Converter - Text/Data to PDF
Fără interfață grafică - doar funcții pentru conversie
Suportă: TXT, CSV, TSV, JSON, XML
"""

from pathlib import Path
import sys
import runtime_bootstrap

runtime_bootstrap.configure()

from engines import reportlab_engine


# ============================================================
# FUNCȚII PRINCIPALE
# ============================================================

def convert_to_pdf(input_file, output_file=None, progress_callback=None):
    """
    Convert a supported file to PDF.
    
    Args:
        input_file: Path to the input file (.txt, .csv, .tsv, .json, .xml)
        output_file: Path to the output PDF file (optional)
        progress_callback: Function to call with progress updates (0.0 - 1.0)
    
    Returns:
        Path to the output PDF file
    
    Raises:
        FileNotFoundError: If input file doesn't exist
        ValueError: If input file is not a supported format
        RuntimeError: If conversion fails
    """
    return reportlab_engine.convert_to_pdf(input_file, output_file, progress_callback)


def batch_convert_to_pdf(input_files, output_folder=None, progress_callback=None):
    """
    Convert multiple files to PDF.
    
    Args:
        input_files: List of input file paths
        output_folder: Output folder path (optional)
        progress_callback: Function to call with progress updates (0.0 - 1.0)
    
    Returns:
        List of output PDF paths
    """
    return reportlab_engine.batch_convert_to_pdf(input_files, output_folder, progress_callback)


# ============================================================
# FUNCȚII DE UTILITATE
# ============================================================

def get_supported_formats():
    """Return all supported formats."""
    return reportlab_engine.supported_formats()


def get_supported_text_formats():
    """Return supported text formats."""
    return reportlab_engine.get_supported_text_formats()


def get_supported_data_formats():
    """Return supported data formats."""
    return reportlab_engine.get_supported_data_formats()


def is_supported_file(file_path):
    """Check if the file is supported."""
    return reportlab_engine.is_reportlab_format(file_path)


def get_format_type(file_path):
    """Get the format type of the file."""
    return reportlab_engine.get_format_type(file_path)


def get_engine_info():
    """Get information about the conversion engine."""
    return {
        "name": reportlab_engine.get_engine_name(),
        "supported_formats": reportlab_engine.supported_formats(),
        "reportlab_installed": reportlab_engine.is_reportlab_installed()
    }


# ============================================================
# FUNCȚII PENTRU LINIA DE COMANDĂ
# ============================================================

def main():
    """Main entry point for command line usage."""
    import argparse
    
    parser = argparse.ArgumentParser(
        description="Convert TXT, CSV, TSV, JSON, XML files to PDF using ReportLab",
        epilog="Supported formats: .txt, .csv, .tsv, .json, .xml"
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
        print("FormatForge - ReportLab Converter")
        print("=" * 50)
        print(f"Engine: {info['name']}")
        print(f"ReportLab installed: {'✅' if info['reportlab_installed'] else '❌'}")
        print(f"Supported formats:")
        print(f"  Text: {', '.join(get_supported_text_formats())}")
        print(f"  Data: {', '.join(get_supported_data_formats())}")
        print("=" * 50)
        
        if not info['reportlab_installed']:
            print("\n⚠️  ReportLab not installed!")
            print("   Run: pip install reportlab")
        
        return
    
    # Batch conversion
    if args.batch:
        if not args.output_folder:
            print("Error: --output-folder is required for batch conversion")
            sys.exit(1)
        
        results = batch_convert_to_pdf(args.batch, args.output_folder)
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
        result = convert_to_pdf(args.input, args.output)
        print(f"✅ Success! Output: {result}")
    except Exception as e:
        print(f"❌ Error: {e}")
        sys.exit(1)


if __name__ == "__main__":
    main()
