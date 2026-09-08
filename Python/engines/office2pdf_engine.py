"""
Office to PDF Conversion Engine
Suportă: .docx, .xlsx, .pptx, .doc, .xls, .ppt

Metode de conversie (în ordinea preferinței):
1. COM Automation (Microsoft Office) - calitate excelentă
2. office2pdf - calitate bună
3. Python libraries - calitate medie (fallback)
"""

from pathlib import Path
import sys
import os
import subprocess
import tempfile

# ============================================================
# FORMATURI SUPORTATE
# ============================================================

WORD_FORMATS = {
    ".doc": "Word Document",
    ".docx": "Word Document",
}

EXCEL_FORMATS = {
    ".xls": "Excel Spreadsheet",
    ".xlsx": "Excel Spreadsheet",
}

POWERPOINT_FORMATS = {
    ".ppt": "PowerPoint Presentation",
    ".pptx": "PowerPoint Presentation",
}

OFFICE_FORMATS = {**WORD_FORMATS, **EXCEL_FORMATS, **POWERPOINT_FORMATS}


# ============================================================
# DETECȚIE FORMAT
# ============================================================

def get_file_extension(file_path):
    return Path(file_path).suffix.lower()


def is_word_format(file_path):
    return get_file_extension(file_path) in WORD_FORMATS


def is_excel_format(file_path):
    return get_file_extension(file_path) in EXCEL_FORMATS


def is_powerpoint_format(file_path):
    return get_file_extension(file_path) in POWERPOINT_FORMATS


def is_office_format(file_path):
    return get_file_extension(file_path) in OFFICE_FORMATS


def get_format_type(file_path):
    ext = get_file_extension(file_path)
    return OFFICE_FORMATS.get(ext, None)


# ============================================================
# VALIDARE
# ============================================================

def validate_input(input_file):
    if not input_file:
        raise ValueError("Input file path is required.")

    input_path = Path(input_file)
    if not input_path.exists():
        raise FileNotFoundError(f"Input file not found: {input_file}")
    if not input_path.is_file():
        raise ValueError(f"Input path is not a file: {input_file}")
    if not is_office_format(input_file):
        raise ValueError(
            f"Input file '{Path(input_file).name}' is not a supported Microsoft Office format.\n"
            f"Supported formats: {', '.join(OFFICE_FORMATS.keys())}"
        )
    return input_path


def get_output_file(input_file, output_file=None):
    input_path = Path(input_file)
    if output_file is None:
        output_path = input_path.with_suffix(".pdf")
    else:
        output_path = Path(output_file)
    output_path.parent.mkdir(parents=True, exist_ok=True)
    return output_path


# ============================================================
# METODA 1: COM AUTOMATION (Microsoft Office)
# Cea mai bună calitate - păstrează formatarea perfectă
# ============================================================

def convert_with_com_automation(input_file, output_file, progress_callback=None):
    """
    Convertește folosind COM Automation.
    Necesită: Windows + Microsoft Office instalat.
    """
    try:
        import comtypes.client
    except ImportError:
        raise RuntimeError("comtypes not installed. Run: pip install comtypes")
    
    input_path = Path(input_file)
    output_path = Path(output_file)
    ext = input_path.suffix.lower()
    
    if progress_callback:
        progress_callback(0.1)
    
    try:
        # ============================================================
        # WORD
        # ============================================================
        if ext in [".docx", ".doc"]:
            print("   Using: Microsoft Word (COM Automation)")
            
            word = comtypes.client.CreateObject("Word.Application")
            word.Visible = False
            word.DisplayAlerts = False
            
            if progress_callback:
                progress_callback(0.3)
            
            doc = word.Documents.Open(str(input_path))
            
            if progress_callback:
                progress_callback(0.6)
            
            # Salvează ca PDF (17 = wdFormatPDF)
            doc.SaveAs(str(output_path), FileFormat=17)
            
            if progress_callback:
                progress_callback(0.8)
            
            doc.Close()
            word.Quit()
        
        # ============================================================
        # EXCEL
        # ============================================================
        elif ext in [".xlsx", ".xls"]:
            print("   Using: Microsoft Excel (COM Automation)")
            
            excel = comtypes.client.CreateObject("Excel.Application")
            excel.Visible = False
            excel.DisplayAlerts = False
            
            if progress_callback:
                progress_callback(0.3)
            
            workbook = excel.Workbooks.Open(str(input_path))
            
            if progress_callback:
                progress_callback(0.6)
            
            # 0 = xlTypePDF
            workbook.ExportAsFixedFormat(0, str(output_path))
            
            if progress_callback:
                progress_callback(0.8)
            
            workbook.Close()
            excel.Quit()
        
        # ============================================================
        # POWERPOINT
        # ============================================================
        elif ext in [".pptx", ".ppt"]:
            print("   Using: Microsoft PowerPoint (COM Automation)")
            
            powerpoint = comtypes.client.CreateObject("PowerPoint.Application")
            powerpoint.Visible = False
            
            if progress_callback:
                progress_callback(0.3)
            
            presentation = powerpoint.Presentations.Open(str(input_path))
            
            if progress_callback:
                progress_callback(0.6)
            
            # 32 = ppSaveAsPDF
            presentation.SaveAs(str(output_path), 32)
            
            if progress_callback:
                progress_callback(0.8)
            
            presentation.Close()
            powerpoint.Quit()
        
        else:
            raise ValueError(f"Unsupported file type: {ext}")
        
        if progress_callback:
            progress_callback(0.9)
        
        # Verifică rezultatul
        if not output_path.exists():
            raise RuntimeError("Output file not created")
        
        if progress_callback:
            progress_callback(1.0)
        
        return output_path
        
    except Exception as e:
        raise RuntimeError(f"COM Automation failed: {e}") from e


# ============================================================
# METODA 2: OFFICE2PDF
# Calitate bună, dar poate avea probleme cu formatarea
# ============================================================

def convert_with_office2pdf(input_file, output_file, progress_callback=None):
    """
    Convertește folosind office2pdf.
    """
    try:
        from office2pdf import convert_path
    except ImportError:
        raise RuntimeError("office2pdf not installed. Run: pip install office2pdf")
    
    input_path = Path(input_file)
    output_path = Path(output_file)
    
    if progress_callback:
        progress_callback(0.2)
    
    print("   Using: office2pdf")
    
    try:
        result = convert_path(str(input_path))
        
        if progress_callback:
            progress_callback(0.7)
        
        # Scrie rezultatul
        if hasattr(result, 'pdf'):
            output_path.write_bytes(result.pdf)
        elif hasattr(result, 'data'):
            output_path.write_bytes(result.data)
        else:
            output_path.write_bytes(result)
        
        if progress_callback:
            progress_callback(0.9)
        
        if not output_path.exists():
            raise RuntimeError("Output file not created")
        
        if progress_callback:
            progress_callback(1.0)
        
        return output_path
        
    except Exception as e:
        raise RuntimeError(f"office2pdf conversion failed: {e}") from e


# ============================================================
# METODA 3: PYTHON LIBRARIES (FALLBACK)
# Calitate medie - doar text de bază
# ============================================================

def convert_with_python_libs(input_file, output_file, progress_callback=None):
    """
    Convertește folosind librării Python (text de bază).
    """
    input_path = Path(input_file)
    output_path = Path(output_file)
    ext = input_path.suffix.lower()
    
    if progress_callback:
        progress_callback(0.2)
    
    try:
        from reportlab.pdfgen import canvas
        from reportlab.lib.pagesizes import A4, landscape
        from reportlab.lib.units import mm
        from reportlab.pdfbase import pdfmetrics
        from reportlab.pdfbase.ttfonts import TTFont
        import textwrap
        
        # Înregistrează un font care suportă diacritice
        try:
            pdfmetrics.registerFont(TTFont('Arial', 'arial.ttf'))
            font_name = 'Arial'
        except:
            font_name = 'Helvetica'
        
        # ============================================================
        # WORD (DOCX)
        # ============================================================
        if ext == ".docx":
            try:
                from docx import Document
                print("   Using: python-docx + reportlab")
                
                doc = Document(input_path)
                
                c = canvas.Canvas(str(output_path), pagesize=A4)
                width, height = A4
                x = 20 * mm
                y = height - 20 * mm
                c.setFont(font_name, 10)
                line_height = 5 * mm
                
                for paragraph in doc.paragraphs:
                    if paragraph.text.strip():
                        lines = textwrap.wrap(paragraph.text, width=80)
                        for line in lines:
                            if y < 20 * mm:
                                c.showPage()
                                y = height - 20 * mm
                                c.setFont(font_name, 10)
                            c.drawString(x, y, line)
                            y -= line_height
                        y -= 2 * mm  # spațiu între paragrafe
                
                c.save()
                
            except ImportError:
                raise RuntimeError("python-docx not installed. Run: pip install python-docx")
        
        # ============================================================
        # EXCEL (XLSX)
        # ============================================================
        elif ext == ".xlsx":
            try:
                import openpyxl
                print("   Using: openpyxl + reportlab")
                
                workbook = openpyxl.load_workbook(input_path, data_only=True)
                sheet = workbook.active
                
                c = canvas.Canvas(str(output_path), pagesize=landscape(A4))
                width, height = landscape(A4)
                x = 15 * mm
                y = height - 20 * mm
                c.setFont(font_name, 8)
                
                col_widths = [30 * mm] * 10  # lățime aproximativă pentru coloane
                row_height = 5 * mm
                
                for row in sheet.iter_rows(values=True):
                    x = 15 * mm
                    for cell in row:
                        if cell is not None:
                            text = str(cell)
                            if len(text) > 20:
                                text = text[:20] + "..."
                            if y < 20 * mm:
                                c.showPage()
                                y = height - 20 * mm
                                c.setFont(font_name, 8)
                            c.drawString(x, y, text)
                        x += 30 * mm
                    y -= row_height
                
                c.save()
                
            except ImportError:
                raise RuntimeError("openpyxl not installed. Run: pip install openpyxl")
        
        # ============================================================
        # POWERPOINT (PPTX)
        # ============================================================
        elif ext == ".pptx":
            try:
                from pptx import Presentation
                print("   Using: python-pptx + reportlab")
                
                prs = Presentation(input_path)
                
                c = canvas.Canvas(str(output_path), pagesize=A4)
                width, height = A4
                x = 20 * mm
                y = height - 20 * mm
                c.setFont(font_name, 12)
                
                slide_count = 0
                for slide in prs.slides:
                    slide_count += 1
                    if slide_count > 1:
                        c.showPage()
                        y = height - 20 * mm
                        c.setFont(font_name, 12)
                    
                    # Titlu slide
                    if slide.shapes.title:
                        title = slide.shapes.title.text
                        c.setFont(font_name, 14)
                        c.drawString(x, y, f"Slide {slide_count}: {title[:50]}")
                        y -= 10 * mm
                        c.setFont(font_name, 12)
                    
                    # Conținut
                    for shape in slide.shapes:
                        if hasattr(shape, "text") and shape.text.strip() and shape != slide.shapes.title:
                            lines = shape.text.split('\n')
                            for line in lines:
                                if y < 20 * mm:
                                    c.showPage()
                                    y = height - 20 * mm
                                    c.setFont(font_name, 12)
                                if line.strip():
                                    c.drawString(x + 5 * mm, y, line[:80])
                                y -= 6 * mm
                            y -= 2 * mm
                
                c.save()
                
            except ImportError:
                raise RuntimeError("python-pptx not installed. Run: pip install python-pptx")
        
        else:
            raise ValueError(f"Unsupported file type for Python conversion: {ext}")
        
        if progress_callback:
            progress_callback(0.9)
        
        if not output_path.exists():
            raise RuntimeError("Output file not created")
        
        if progress_callback:
            progress_callback(1.0)
        
        return output_path
        
    except Exception as e:
        raise RuntimeError(f"Python conversion failed: {e}") from e


# ============================================================
# VERIFICĂ DISPONIBILITATEA
# ============================================================

def is_office_installed():
    """Verifică dacă Microsoft Office este instalat."""
    if sys.platform != "win32":
        return False
    
    try:
        import comtypes.client
        # Încearcă să creeze un obiect Word
        word = comtypes.client.CreateObject("Word.Application")
        word.Quit()
        return True
    except:
        return False


def is_office2pdf_installed():
    try:
        import office2pdf
        return True
    except:
        return False


# ============================================================
# FUNCȚIA PRINCIPALĂ DE CONVERSIE
# ============================================================

def convert_to_pdf(input_file, output_file=None, progress_callback=None):
    """
    Convert an Office file to PDF.
    Alege automat cea mai bună metodă disponibilă.
    """
    input_path = validate_input(input_file)
    output_path = get_output_file(input_file, output_file)
    
    if progress_callback:
        progress_callback(0.05)
    
    file_name = input_path.name
    format_type = get_format_type(str(input_path))
    
    print(f"\n📄 Converting: {file_name}")
    print(f"   Type: {format_type}")
    print(f"   Output: {output_path}")
    print("   " + "-" * 50)
    
    try:
        # ============================================================
        # METODA 1: COM Automation (Microsoft Office) - CEA MAI BUNĂ
        # ============================================================
        if sys.platform == "win32" and is_office_installed():
            try:
                result = convert_with_com_automation(
                    str(input_path),
                    str(output_path),
                    progress_callback
                )
                print("   ✅ Conversion successful (COM Automation)")
                return result
            except Exception as e:
                print(f"   ⚠️  COM Automation failed: {e}")
                print("   🔄 Trying next method...")
        
        # ============================================================
        # METODA 2: office2pdf
        # ============================================================
        if is_office2pdf_installed():
            try:
                result = convert_with_office2pdf(
                    str(input_path),
                    str(output_path),
                    progress_callback
                )
                print("   ✅ Conversion successful (office2pdf)")
                return result
            except Exception as e:
                print(f"   ⚠️  office2pdf failed: {e}")
                print("   🔄 Trying fallback method...")
        
        # ============================================================
        # METODA 3: Python Libraries (FALLBACK)
        # ============================================================
        print("   Using: Python libraries (basic conversion)")
        result = convert_with_python_libs(
            str(input_path),
            str(output_path),
            progress_callback
        )
        print("   ✅ Conversion successful (Python libraries)")
        
        size_mb = output_path.stat().st_size / (1024 * 1024)
        print(f"   📁 Size: {size_mb:.2f} MB")
        
        return result
        
    except Exception as error:
        error_msg = f"Failed to convert '{file_name}' to PDF: {error}"
        print(f"   ❌ Error: {error_msg}")
        raise RuntimeError(error_msg) from error


# ============================================================
# BATCH CONVERSION
# ============================================================

def batch_convert_to_pdf(input_files, output_folder=None, progress_callback=None):
    """Convert multiple Office files to PDF."""
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


# ============================================================
# INFORMAȚII
# ============================================================

def get_engine_info():
    return {
        "name": "office2pdf_engine",
        "version": "2.0.0",
        "supported_formats": list(OFFICE_FORMATS.keys()),
        "office_installed": is_office_installed(),
        "office2pdf_installed": is_office2pdf_installed()
    }


def supported_formats():
    return list(OFFICE_FORMATS.keys())


# ============================================================
# TESTARE
# ============================================================

if __name__ == "__main__":
    print("=" * 60)
    print("Office2PDF Engine v2.0")
    print("=" * 60)
    
    info = get_engine_info()
    print(f"Supported formats: {', '.join(info['supported_formats'])}")
    print(f"Microsoft Office installed: {'✅' if info['office_installed'] else '❌'}")
    print(f"office2pdf installed: {'✅' if info['office2pdf_installed'] else '❌'}")
    
    if not info['office_installed'] and not info['office2pdf_installed']:
        print("\n⚠️  WARNING: No conversion engine available!")
        print("   Install Microsoft Office OR run: pip install office2pdf")
    elif info['office_installed']:
        print("\n✅ Using Microsoft Office (best quality)")
    elif info['office2pdf_installed']:
        print("\n✅ Using office2pdf (good quality)")
    
    print("=" * 60)