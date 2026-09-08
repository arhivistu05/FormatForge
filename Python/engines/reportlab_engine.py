"""
ReportLab Engine - Text/Data to PDF
Suportă: TXT, CSV, TSV, JSON, XML
"""

from pathlib import Path
import sys
import json
import xml.etree.ElementTree as ET
from datetime import datetime

try:
    from reportlab.pdfgen import canvas
    from reportlab.lib.pagesizes import A4, landscape
    from reportlab.lib.units import mm
    from reportlab.lib.colors import Color, black, blue, red, grey
    from reportlab.pdfbase import pdfmetrics
    from reportlab.pdfbase.ttfonts import TTFont
    from reportlab.lib.styles import getSampleStyleSheet
    from reportlab.platypus import SimpleDocTemplate, Paragraph, Spacer, Table, TableStyle
    from reportlab.lib import colors
    HAS_REPORTLAB = True
except ImportError:
    HAS_REPORTLAB = False
    print("⚠️  ReportLab not installed.")
    print("   Install with: pip install reportlab")


TEXT_FORMATS = {
    ".txt": "Text Document",
}

DATA_FORMATS = {
    ".csv": "CSV Document",
    ".tsv": "TSV Document", 
    ".json": "JSON Document",
    ".xml": "XML Document",
}

REPORTLAB_FORMATS = {
    **TEXT_FORMATS,
    **DATA_FORMATS,
}


def get_file_extension(file_path):
    return Path(file_path).suffix.lower()


def is_text_format(file_path):
    return get_file_extension(file_path) in TEXT_FORMATS


def is_data_format(file_path):
    return get_file_extension(file_path) in DATA_FORMATS


def is_reportlab_format(file_path):
    return get_file_extension(file_path) in REPORTLAB_FORMATS


def get_format_type(file_path):
    extension = get_file_extension(file_path)
    if extension in TEXT_FORMATS:
        return TEXT_FORMATS[extension]
    elif extension in DATA_FORMATS:
        return DATA_FORMATS[extension]
    return None


def validate_input_file(file_path):
    input_file = Path(file_path)

    if not input_file.exists():
        raise FileNotFoundError(f"Input file '{file_path}' does not exist.")

    if not input_file.is_file():
        raise ValueError(f"Input path '{file_path}' is not a file.")

    if not is_reportlab_format(file_path):
        raise ValueError(
            f"Input file '{file_path}' is not a supported ReportLab format.\n"
            f"Supported formats: {', '.join(REPORTLAB_FORMATS.keys())}"
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


# ============================================================
# CONVERTORI PENTRU FIECARE FORMAT
# ============================================================

def convert_txt_to_pdf(input_file, output_path, progress_callback=None):
    """Convertește fișier TXT la PDF."""
    if progress_callback:
        progress_callback(0.2)
    
    pdf = canvas.Canvas(str(output_path), pagesize=A4)
    width, height = A4
    x = 20 * mm
    y = height - 20 * mm
    line_height = 5 * mm
    
    # Înregistrează un font care suportă diacritice
    try:
        pdfmetrics.registerFont(TTFont('Arial', 'arial.ttf'))
        pdf.setFont('Arial', 10)
    except:
        pdf.setFont('Helvetica', 10)
    
    # Adaugă titlu
    pdf.setFont('Helvetica-Bold', 14)
    pdf.drawString(x, y, f"Document: {input_file.name}")
    y -= 10 * mm
    pdf.setFont('Helvetica', 10)
    
    if progress_callback:
        progress_callback(0.4)
    
    # Citește și scrie conținutul
    with open(input_file, 'r', encoding='utf-8', errors='ignore') as f:
        for line in f:
            line = line.rstrip('\n\r')
            
            if y < 20 * mm:
                pdf.showPage()
                y = height - 20 * mm
                pdf.setFont('Helvetica', 10)
            
            # Trunchiază linii prea lungi
            if len(line) > 100:
                line = line[:97] + "..."
            
            pdf.drawString(x, y, line)
            y -= line_height
    
    if progress_callback:
        progress_callback(0.8)
    
    # Adaugă număr de pagini
    page_count = pdf.getPageNumber()
    for i in range(1, page_count + 1):
        pdf.showPage()
    
    pdf.save()
    
    if progress_callback:
        progress_callback(1.0)
    
    return output_path


def convert_csv_to_pdf(input_file, output_path, progress_callback=None):
    """Convertește fișier CSV la PDF (ca tabel)."""
    import csv
    
    if progress_callback:
        progress_callback(0.2)
    
    # Citește CSV
    rows = []
    with open(input_file, 'r', encoding='utf-8', errors='ignore') as f:
        reader = csv.reader(f)
        for row in reader:
            rows.append(row)
    
    if not rows:
        raise ValueError("CSV file is empty")
    
    if progress_callback:
        progress_callback(0.4)
    
    # Creează PDF cu tabel
    doc = SimpleDocTemplate(str(output_path), pagesize=landscape(A4))
    elements = []
    
    # Titlu
    styles = getSampleStyleSheet()
    title = Paragraph(f"CSV Document: {input_file.name}", styles['Title'])
    elements.append(title)
    elements.append(Spacer(1, 10 * mm))
    
    # Tabel
    if rows:
        # Determină lățimea coloanelor
        col_count = max(len(row) for row in rows)
        col_widths = [80] * min(col_count, 10)
        
        # Creează tabelul
        table = Table(rows, colWidths=col_widths)
        table.setStyle(TableStyle([
            ('BACKGROUND', (0, 0), (-1, 0), colors.grey),
            ('TEXTCOLOR', (0, 0), (-1, 0), colors.whitesmoke),
            ('ALIGN', (0, 0), (-1, -1), 'CENTER'),
            ('FONTNAME', (0, 0), (-1, 0), 'Helvetica-Bold'),
            ('FONTSIZE', (0, 0), (-1, 0), 10),
            ('BOTTOMPADDING', (0, 0), (-1, 0), 6),
            ('BACKGROUND', (0, 1), (-1, -1), colors.white),
            ('GRID', (0, 0), (-1, -1), 1, colors.black),
            ('FONTSIZE', (0, 1), (-1, -1), 8),
        ]))
        elements.append(table)
    
    if progress_callback:
        progress_callback(0.8)
    
    doc.build(elements)
    
    if progress_callback:
        progress_callback(1.0)
    
    return output_path


def convert_json_to_pdf(input_file, output_path, progress_callback=None):
    """Convertește fișier JSON la PDF (formatat)."""
    if progress_callback:
        progress_callback(0.2)
    
    # Citește JSON
    with open(input_file, 'r', encoding='utf-8', errors='ignore') as f:
        data = json.load(f)
    
    if progress_callback:
        progress_callback(0.4)
    
    # Creează PDF
    pdf = canvas.Canvas(str(output_path), pagesize=A4)
    width, height = A4
    x = 20 * mm
    y = height - 20 * mm
    line_height = 5 * mm
    
    try:
        pdfmetrics.registerFont(TTFont('Arial', 'arial.ttf'))
        pdf.setFont('Arial', 10)
    except:
        pdf.setFont('Helvetica', 10)
    
    # Titlu
    pdf.setFont('Helvetica-Bold', 14)
    pdf.drawString(x, y, f"JSON Document: {input_file.name}")
    y -= 10 * mm
    pdf.setFont('Helvetica', 10)
    
    if progress_callback:
        progress_callback(0.6)
    
    # Formatează JSON
    json_str = json.dumps(data, indent=2, ensure_ascii=False)
    
    # Scrie linie cu linie
    for line in json_str.split('\n'):
        if y < 20 * mm:
            pdf.showPage()
            y = height - 20 * mm
            pdf.setFont('Helvetica', 10)
        
        # Trunchiază linii prea lungi
        if len(line) > 100:
            line = line[:97] + "..."
        
        pdf.drawString(x, y, line)
        y -= line_height
    
    if progress_callback:
        progress_callback(0.8)
    
    pdf.save()
    
    if progress_callback:
        progress_callback(1.0)
    
    return output_path


def convert_xml_to_pdf(input_file, output_path, progress_callback=None):
    """Convertește fișier XML la PDF (formatat)."""
    if progress_callback:
        progress_callback(0.2)
    
    # Citește XML
    try:
        tree = ET.parse(input_file)
        root = tree.getroot()
    except ET.ParseError as e:
        # Dacă XML-ul este invalid, citește ca text
        with open(input_file, 'r', encoding='utf-8', errors='ignore') as f:
            content = f.read()
        return convert_text_to_pdf_with_content(content, input_file.name, output_path, progress_callback)
    
    if progress_callback:
        progress_callback(0.4)
    
    # Formatează XML
    xml_str = ET.tostring(root, encoding='unicode', method='xml')
    
    # Creează PDF
    pdf = canvas.Canvas(str(output_path), pagesize=A4)
    width, height = A4
    x = 20 * mm
    y = height - 20 * mm
    line_height = 5 * mm
    
    try:
        pdfmetrics.registerFont(TTFont('Arial', 'arial.ttf'))
        pdf.setFont('Arial', 10)
    except:
        pdf.setFont('Helvetica', 10)
    
    # Titlu
    pdf.setFont('Helvetica-Bold', 14)
    pdf.drawString(x, y, f"XML Document: {input_file.name}")
    y -= 10 * mm
    pdf.setFont('Helvetica', 10)
    
    if progress_callback:
        progress_callback(0.6)
    
    # Scrie linie cu linie
    for line in xml_str.split('\n'):
        if y < 20 * mm:
            pdf.showPage()
            y = height - 20 * mm
            pdf.setFont('Helvetica', 10)
        
        if len(line) > 100:
            line = line[:97] + "..."
        
        pdf.drawString(x, y, line)
        y -= line_height
    
    if progress_callback:
        progress_callback(0.8)
    
    pdf.save()
    
    if progress_callback:
        progress_callback(1.0)
    
    return output_path


def convert_text_to_pdf_with_content(content, filename, output_path, progress_callback=None):
    """Convertește conținut text la PDF."""
    if progress_callback:
        progress_callback(0.2)
    
    pdf = canvas.Canvas(str(output_path), pagesize=A4)
    width, height = A4
    x = 20 * mm
    y = height - 20 * mm
    line_height = 5 * mm
    
    try:
        pdfmetrics.registerFont(TTFont('Arial', 'arial.ttf'))
        pdf.setFont('Arial', 10)
    except:
        pdf.setFont('Helvetica', 10)
    
    pdf.setFont('Helvetica-Bold', 14)
    pdf.drawString(x, y, f"Document: {filename}")
    y -= 10 * mm
    pdf.setFont('Helvetica', 10)
    
    if progress_callback:
        progress_callback(0.4)
    
    for line in content.split('\n'):
        if y < 20 * mm:
            pdf.showPage()
            y = height - 20 * mm
            pdf.setFont('Helvetica', 10)
        
        if len(line) > 100:
            line = line[:97] + "..."
        
        pdf.drawString(x, y, line)
        y -= line_height
    
    if progress_callback:
        progress_callback(0.8)
    
    pdf.save()
    
    if progress_callback:
        progress_callback(1.0)
    
    return output_path


# ============================================================
# FUNCȚIA PRINCIPALĂ DE CONVERSIE
# ============================================================

def convert_to_pdf(input_file, output_file=None, progress_callback=None):
    """
    Convert a supported document to PDF.
    """
    if progress_callback:
        progress_callback(0.1)
    
    input_path = validate_input_file(input_file)
    output_path = get_output_file(input_file, output_file)
    
    if progress_callback:
        progress_callback(0.15)
    
    if not HAS_REPORTLAB:
        raise RuntimeError(
            "ReportLab is not installed.\n"
            "Please install it with: pip install reportlab"
        )
    
    ext = input_path.suffix.lower()
    
    print(f"\n📄 Converting: {input_path.name}")
    print(f"   Type: {get_format_type(str(input_path))}")
    print(f"   Output: {output_path}")
    print(f"   Using: ReportLab")
    
    if progress_callback:
        progress_callback(0.2)
    
    try:
        if ext == ".txt":
            result = convert_txt_to_pdf(input_path, output_path, progress_callback)
        elif ext == ".csv":
            result = convert_csv_to_pdf(input_path, output_path, progress_callback)
        elif ext == ".tsv":
            # TSV - același ca CSV dar cu delimitator tab
            result = convert_csv_to_pdf(input_path, output_path, progress_callback)
        elif ext == ".json":
            result = convert_json_to_pdf(input_path, output_path, progress_callback)
        elif ext == ".xml":
            result = convert_xml_to_pdf(input_path, output_path, progress_callback)
        else:
            raise ValueError(f"Unsupported format: {ext}")
        
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


def batch_convert_to_pdf(input_files, output_folder=None, progress_callback=None):
    """Convert multiple files to PDF."""
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


def get_engine_name():
    return "ReportLab"


def supported_formats():
    return list(REPORTLAB_FORMATS.keys())


def get_supported_text_formats():
    return sorted(TEXT_FORMATS.keys())


def get_supported_data_formats():
    return sorted(DATA_FORMATS.keys())


def is_reportlab_installed():
    return HAS_REPORTLAB


# ============================================================
# TESTARE
# ============================================================

if __name__ == "__main__":
    print("=" * 50)
    print("ReportLab Engine")
    print("=" * 50)
    print(f"ReportLab installed: {'✅' if HAS_REPORTLAB else '❌'}")
    print(f"Supported formats:")
    print(f"  Text: {', '.join(get_supported_text_formats())}")
    print(f"  Data: {', '.join(get_supported_data_formats())}")
    
    if not HAS_REPORTLAB:
        print("\n⚠️  ReportLab not installed!")
        print("   Run: pip install reportlab")
    
    print("=" * 50)