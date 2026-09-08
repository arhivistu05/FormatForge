"""
HTML to PDF Engine
Folosește Playwright (Chromium) pentru conversie
"""

from pathlib import Path
import sys

try:
    from playwright.sync_api import sync_playwright
    HAS_PLAYWRIGHT = True
except ImportError:
    HAS_PLAYWRIGHT = False
    sync_playwright = None
    print("⚠️  Playwright not installed.")
    print("   Install with:")
    print("   pip install playwright")
    print("   python -m playwright install")


HTML_FORMATS = {
    ".html": "HTML Document",
    ".htm": "HTML Document",
}


def get_file_extension(file_path):
    return Path(file_path).suffix.lower()


def is_html_format(file_path):
    return get_file_extension(file_path) in HTML_FORMATS


def is_html_or_htm_format(file_path):
    return get_file_extension(file_path) in HTML_FORMATS


def get_format_type(file_path):
    extension = get_file_extension(file_path)
    return HTML_FORMATS.get(extension, None)


def validate_input_file(file_path):
    input_file = Path(file_path)

    if not input_file.exists():
        raise FileNotFoundError(f"Input file '{file_path}' does not exist.")

    if not input_file.is_file():
        raise ValueError(f"Input path '{file_path}' is not a file.")

    if not is_html_or_htm_format(file_path):
        raise ValueError(
            f"Input file '{file_path}' is not a supported HTML format.\n"
            f"Supported formats: {', '.join(HTML_FORMATS.keys())}"
        )

    return input_file


def get_output_file(input_file, output_file=None):
    input_path = Path(input_file)

    if output_file is None:
        output_path = input_path.with_suffix(".pdf")
    else:
        output_path = Path(output_file)

    output_path.parent.mkdir(parents=True, exist_ok=True)

    return output_path


def convert_html_to_pdf(input_file, output_file=None, progress_callback=None):
    """
    Convert HTML/HTM to PDF using Playwright (Chromium).
    """
    if progress_callback:
        progress_callback(0.1)
    
    input_path = validate_input_file(input_file)
    output_path = get_output_file(input_file, output_file)
    
    if progress_callback:
        progress_callback(0.2)
    
    if not HAS_PLAYWRIGHT:
        raise RuntimeError(
            "Playwright is not installed.\n"
            "Please install it with:\n"
            "  pip install playwright\n"
            "  python -m playwright install"
        )
    
    print(f"\n📄 Converting: {input_path.name}")
    print(f"   Type: {get_format_type(str(input_path))}")
    print(f"   Output: {output_path}")
    print("   Using: Playwright (Chromium)")
    
    if progress_callback:
        progress_callback(0.4)
    
    try:
        # Citește conținutul HTML
        html_content = input_path.read_text(encoding='utf-8')
        
        if progress_callback:
            progress_callback(0.6)
        
        # Convertește HTML la PDF
        with sync_playwright() as p:
            browser = p.chromium.launch(headless=True)
            page = browser.new_page()
            
            page.set_viewport_size({"width": 1200, "height": 800})
            
            if progress_callback:
                progress_callback(0.7)
            
            page.set_content(html_content)
            
            if progress_callback:
                progress_callback(0.8)
            
            page.pdf(
                path=str(output_path),
                format="A4",
                print_background=True,
                margin={
                    "top": "10mm",
                    "bottom": "10mm",
                    "left": "10mm",
                    "right": "10mm"
                }
            )
            
            browser.close()
        
        if progress_callback:
            progress_callback(0.9)
        
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


def batch_convert_html_to_pdf(input_files, output_folder=None, progress_callback=None):
    """Convert multiple HTML files to PDF."""
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
            result = convert_html_to_pdf(input_file, str(output_file), progress_callback)
        else:
            result = convert_html_to_pdf(input_file, None, progress_callback)
        
        results.append(result)
    
    if progress_callback:
        progress_callback(1.0)
    
    return results


def get_engine_name():
    return "Playwright (Chromium)"


def supported_formats():
    return list(HTML_FORMATS.keys())


def get_supported_formats():
    return sorted(HTML_FORMATS.keys())


def is_playwright_installed():
    return HAS_PLAYWRIGHT


# ============================================================
# TESTARE
# ============================================================

if __name__ == "__main__":
    print("=" * 50)
    print("HTML to PDF Engine")
    print("=" * 50)
    print(f"Playwright installed: {'✅' if HAS_PLAYWRIGHT else '❌'}")
    print(f"Supported formats: {', '.join(supported_formats())}")
    
    if not HAS_PLAYWRIGHT:
        print("\n⚠️  Playwright not installed!")
        print("   Run the following commands:")
        print("   pip install playwright")
        print("   python -m playwright install")
    
    print("=" * 50)