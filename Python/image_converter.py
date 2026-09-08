"""
Image Converter - Image to PDF
Cu interfaÃˆâ€ºÃ„Æ’ graficÃ„Æ’ completÃ„Æ’
SuportÃ„Æ’: JPG, PNG, BMP, GIF, TIFF, WEBP
"""

from pathlib import Path
import sys
import runtime_bootstrap

runtime_bootstrap.configure()

try:
    import tkinter as tk
    from tkinter import filedialog, messagebox, ttk
except ImportError:
    tk = None
    filedialog = messagebox = ttk = None

from engines import image2pdf_engine


# ============================================================
# GUI APPLICATION
# ============================================================

class ImageConverterApp:
    """Full GUI application for Image to PDF conversion."""
    
    def __init__(self):
        if tk is None:
            raise RuntimeError("Tkinter is not available in this Python runtime.")

        self.root = tk.Tk()
        self.root.title("FormatForge - Image to PDF Converter")
        self.root.geometry("700x500")
        self.root.minsize(600, 400)
        
        self.input_files = []
        self.output_folder = None
        self.merge_mode = False
        
        self.colors = {
            'primary': '#E91E63',      # Roz pentru imagini
            'success': '#4CAF50',
            'error': '#f44336',
            'bg': '#f5f5f5',
            'white': '#ffffff'
        }
        
        self._create_widgets()
        self._update_status()
        self._center_window()
    
    def _center_window(self):
        self.root.update_idletasks()
        width = self.root.winfo_width()
        height = self.root.winfo_height()
        x = (self.root.winfo_screenwidth() // 2) - (width // 2)
        y = (self.root.winfo_screenheight() // 2) - (height // 2)
        self.root.geometry(f'{width}x{height}+{x}+{y}')
    
    def _create_widgets(self):
        # Header
        header = tk.Frame(self.root, bg=self.colors['primary'], height=70)
        header.pack(fill="x")
        header.pack_propagate(False)
        
        tk.Label(
            header,
            text="Ã°Å¸â€“Â¼Ã¯Â¸Â  FormatForge - Image to PDF Converter",
            font=("Segoe UI", 16, "bold"),
            fg="white",
            bg=self.colors['primary']
        ).pack(expand=True)
        
        tk.Label(
            header,
            text="Convert JPG, PNG, BMP, GIF, TIFF, WEBP to PDF",
            font=("Segoe UI", 9),
            fg="white",
            bg=self.colors['primary']
        ).pack()
        
        # Main frame
        main = tk.Frame(self.root, bg=self.colors['bg'])
        main.pack(fill="both", expand=True, padx=15, pady=10)
        
        # Input files
        input_frame = tk.LabelFrame(
            main, 
            text="Ã°Å¸â€œâ€š Input Images", 
            font=("Segoe UI", 10), 
            bg=self.colors['bg'], 
            padx=10, 
            pady=10
        )
        input_frame.pack(fill="both", expand=True, pady=(0, 10))
        
        btn_frame = tk.Frame(input_frame, bg=self.colors['bg'])
        btn_frame.pack(fill="x", pady=(0, 5))
        
        tk.Button(
            btn_frame,
            text="Ã¢Å¾â€¢ Add Images",
            command=self._add_files,
            bg=self.colors['primary'],
            fg="white",
            font=("Segoe UI", 10, "bold"),
            padx=10,
            pady=5,
            cursor="hand2"
        ).pack(side="left", padx=(0, 5))
        
        tk.Button(
            btn_frame,
            text="Ã°Å¸â€”â€˜Ã¯Â¸Â Clear All",
            command=self._clear_files,
            bg=self.colors['error'],
            fg="white",
            font=("Segoe UI", 10, "bold"),
            padx=10,
            pady=5,
            cursor="hand2"
        ).pack(side="left")
        
        self.file_count = tk.Label(
            btn_frame, 
            text="0 images", 
            bg=self.colors['bg'],
            font=("Segoe UI", 10)
        )
        self.file_count.pack(side="right")
        
        # Listbox
        listbox_frame = tk.Frame(input_frame, bg=self.colors['bg'])
        listbox_frame.pack(fill="both", expand=True)
        
        self.listbox = tk.Listbox(
            listbox_frame,
            height=5,
            selectmode=tk.EXTENDED,
            font=("Consolas", 9),
            bg="white",
            relief="solid",
            borderwidth=1
        )
        self.listbox.pack(side="left", fill="both", expand=True)
        
        scrollbar = tk.Scrollbar(listbox_frame)
        scrollbar.pack(side="right", fill="y")
        self.listbox.config(yscrollcommand=scrollbar.set)
        scrollbar.config(command=self.listbox.yview)
        
        # Output folder
        output_frame = tk.LabelFrame(
            main, 
            text="Ã°Å¸â€œÂ Output Settings", 
            font=("Segoe UI", 10), 
            bg=self.colors['bg'], 
            padx=10, 
            pady=10
        )
        output_frame.pack(fill="x", pady=(0, 10))
        
        folder_frame = tk.Frame(output_frame, bg=self.colors['bg'])
        folder_frame.pack(fill="x")
        
        tk.Label(
            folder_frame,
            text="Output Folder:",
            bg=self.colors['bg'],
            font=("Segoe UI", 10),
            width=15,
            anchor="w"
        ).pack(side="left")
        
        self.folder_label = tk.Label(
            folder_frame,
            text="Same as input (default)",
            bg=self.colors['bg'],
            anchor="w",
            font=("Segoe UI", 10)
        )
        self.folder_label.pack(side="left", fill="x", expand=True, padx=(5, 5))
        
        tk.Button(
            folder_frame,
            text="Ã°Å¸â€œâ€š Browse",
            command=self._select_folder,
            bg=self.colors['primary'],
            fg="white",
            font=("Segoe UI", 10, "bold"),
            padx=10,
            pady=3,
            cursor="hand2"
        ).pack(side="right")
        
        # Options
        options_frame = tk.Frame(main, bg=self.colors['bg'])
        options_frame.pack(fill="x", pady=(0, 10))
        
        self.overwrite_var = tk.BooleanVar(value=True)
        tk.Checkbutton(
            options_frame,
            text="Overwrite existing files",
            variable=self.overwrite_var,
            bg=self.colors['bg'],
            font=("Segoe UI", 10)
        ).pack(side="left", padx=(0, 20))
        
        self.merge_var = tk.BooleanVar(value=False)
        tk.Checkbutton(
            options_frame,
            text="Merge all images into single PDF (in order)",
            variable=self.merge_var,
            bg=self.colors['bg'],
            font=("Segoe UI", 10)
        ).pack(side="left")
        
        self.open_folder_var = tk.BooleanVar(value=False)
        tk.Checkbutton(
            options_frame,
            text="Open folder after conversion",
            variable=self.open_folder_var,
            bg=self.colors['bg'],
            font=("Segoe UI", 10)
        ).pack(side="left", padx=(0, 20))
        
        # Progress
        progress_frame = tk.Frame(main, bg=self.colors['bg'])
        progress_frame.pack(fill="x", pady=(0, 10))
        
        self.progress = ttk.Progressbar(
            progress_frame, 
            maximum=100, 
            length=100, 
            mode='determinate'
        )
        self.progress.pack(fill="x")
        
        self.progress_label = tk.Label(
            progress_frame, 
            text="Ready", 
            bg=self.colors['bg'],
            font=("Segoe UI", 10)
        )
        self.progress_label.pack(pady=(5, 0))
        
        # Convert button
        self.convert_btn = tk.Button(
            main,
            text="Ã°Å¸Å¡â‚¬ Convert to PDF",
            command=self._convert,
            bg=self.colors['success'],
            fg="white",
            font=("Segoe UI", 13, "bold"),
            height=2,
            cursor="hand2",
            relief="raised",
            bd=3
        )
        self.convert_btn.pack(fill="x", pady=(0, 5))
        
        # Status
        status_frame = tk.Frame(self.root, bg=self.colors['primary'], height=30)
        status_frame.pack(fill="x", side="bottom")
        status_frame.pack_propagate(False)
        
        self.status = tk.Label(
            status_frame,
            text="Ready - Add images to convert",
            anchor="w",
            padx=10,
            fg="white",
            bg=self.colors['primary'],
            font=("Segoe UI", 10)
        )
        self.status.pack(fill="x", expand=True)
    
    def _add_files(self):
        files = filedialog.askopenfilenames(
            title="Select Images",
            filetypes=[
                ("All Images", "*.jpg;*.jpeg;*.png;*.bmp;*.gif;*.tiff;*.tif;*.webp"),
                ("JPEG", "*.jpg;*.jpeg"),
                ("PNG", "*.png"),
                ("BMP", "*.bmp"),
                ("GIF", "*.gif"),
                ("TIFF", "*.tiff;*.tif"),
                ("WebP", "*.webp"),
                ("All Files", "*.*"),
            ]
        )
        
        if files:
            added = 0
            invalid = []
            for f in files:
                ext = Path(f).suffix.lower()
                if ext in ['.jpg', '.jpeg', '.png', '.bmp', '.gif', '.tiff', '.tif', '.webp']:
                    if str(f) not in self.listbox.get(0, tk.END):
                        self.listbox.insert(tk.END, str(f))
                        added += 1
                else:
                    invalid.append(Path(f).name)
            
            if invalid:
                messagebox.showwarning(
                    "Invalid Files",
                    f"The following files are not supported images:\n\n" + "\n".join(invalid) +
                    f"\n\nSupported formats: .jpg, .jpeg, .png, .bmp, .gif, .tiff, .tif, .webp"
                )
            
            if added > 0:
                self._update_status()
                messagebox.showinfo(
                    "Images Added",
                    f"Added {added} image(s) to the list."
                )
    
    def _clear_files(self):
        if self.listbox.size() > 0:
            if messagebox.askyesno("Clear Files", "Remove all images from the list?"):
                self.listbox.delete(0, tk.END)
                self._update_status()
    
    def _select_folder(self):
        folder = filedialog.askdirectory(title="Select Output Folder")
        if folder:
            self.output_folder = Path(folder)
            self.folder_label.config(text=str(self.output_folder))
    
    def _update_status(self):
        count = self.listbox.size()
        if count == 0:
            self.file_count.config(text="No images selected")
            self.convert_btn.config(state="disabled")
            self.status.config(text="Ready - Add images to convert")
        else:
            self.file_count.config(text=f"{count} image(s) selected")
            self.convert_btn.config(state="normal")
            self.status.config(text=f"Ready - {count} image(s) ready to convert")
        
        self.progress['value'] = 0
        self.progress_label.config(text="Ready")
    
    def _update_progress(self, progress, message=""):
        percent = int(progress * 100)
        self.progress['value'] = percent
        self.progress_label.config(text=message or f"Converting... {percent}%")
        self.root.update_idletasks()
    
    def _convert(self):
        files = list(self.listbox.get(0, tk.END))
        if not files:
            messagebox.showwarning("No Files", "Please add images to convert.")
            return
        
        missing_files = []
        for f in files:
            if not Path(f).exists():
                missing_files.append(Path(f).name)
        
        if missing_files:
            messagebox.showerror(
                "Files Not Found",
                f"The following files were not found:\n\n" + "\n".join(missing_files)
            )
            return
        
        self.convert_btn.config(state="disabled", text="Ã¢ÂÂ³ Converting...")
        self.status.config(text="Converting images...")
        self.progress['value'] = 0
        self.progress_label.config(text="Starting conversion...")
        self.root.update()
        
        try:
            # DacÃ„Æ’ merge mode, combinÃ„Æ’ toate imaginile ÃƒÂ®ntr-un singur PDF
            if self.merge_var.get() and len(files) > 1:
                self._update_progress(0.1, "Merging images...")
                
                output_file = None
                if self.output_folder:
                    input_path = Path(files[0])
                    output_file = self.output_folder / (input_path.stem + "_merged.pdf")
                
                result = image2pdf_engine.merge_images_to_pdf(
                    files,
                    str(output_file) if output_file else None,
                    lambda p: self._update_progress(p, f"Merging... {int(p*100)}%")
                )
                
                self.progress['value'] = 100
                messagebox.showinfo("Conversion Complete", f"Ã¢Å“â€¦ Success!\n\nÃ°Å¸â€œÂ Output: {result}")
                self.status.config(text=f"Done - Merged {len(files)} images")
                
                if self.open_folder_var.get():
                    import os
                    os.startfile(str(result.parent))
            
            else:
                # Conversie individualÃ„Æ’
                results = []
                total = len(files)
                
                for i, file_path in enumerate(files):
                    progress = i / total
                    self._update_progress(
                        progress, 
                        f"Converting {Path(file_path).name}... ({i+1}/{total})"
                    )
                    
                    try:
                        if self.output_folder:
                            input_path = Path(file_path)
                            output_file = self.output_folder / (input_path.stem + ".pdf")
                            
                            if output_file.exists() and not self.overwrite_var.get():
                                response = messagebox.askyesno(
                                    "File Exists",
                                    f"File '{output_file.name}' already exists.\nOverwrite?"
                                )
                                if not response:
                                    results.append(None)
                                    continue
                            
                            result = image2pdf_engine.convert_to_pdf(
                                str(file_path),
                                str(output_file)
                            )
                        else:
                            result = image2pdf_engine.convert_to_pdf(
                                str(file_path),
                                None
                            )
                        
                        results.append(result)
                        
                    except Exception as e:
                        results.append(None)
                        print(f"Error converting {file_path}: {e}")
                        self.status.config(text=f"Error: {Path(file_path).name}")
                
                self.progress['value'] = 100
                
                success = [r for r in results if r is not None]
                failed = len(results) - len(success)
                
                msg = f"Conversion Complete!\n\nÃ¢Å“â€¦ Success: {len(success)}\nÃ¢ÂÅ’ Failed: {failed}"
                
                if success:
                    msg += f"\n\nÃ°Å¸â€œÂ Output folder: {success[0].parent if success else 'N/A'}"
                
                messagebox.showinfo("Conversion Complete", msg)
                
                if self.open_folder_var.get() and success:
                    import os
                    os.startfile(str(success[0].parent))
                
                self.status.config(text=f"Done - {len(success)} image(s) converted")
            
        except Exception as e:
            messagebox.showerror("Error", str(e))
            self.status.config(text="Error during conversion")
        
        finally:
            self.convert_btn.config(state="normal", text="Ã°Å¸Å¡â‚¬ Convert to PDF")
            self._update_status()
            self.progress_label.config(text="Done")
    
    def run(self):
        self.root.mainloop()


# ============================================================
# FUNCÃˆÅ¡II PENTRU LINIA DE COMANDÃ„â€š
# ============================================================

def convert_to_pdf(input_file, output_file=None, progress_callback=None):
    """Convert an image to PDF (linia de comandÃ„Æ’)."""
    return image2pdf_engine.convert_to_pdf(input_file, output_file, progress_callback)


def merge_images_to_pdf(image_files, output_file=None, progress_callback=None):
    """Merge multiple images into a single PDF."""
    return image2pdf_engine.merge_images_to_pdf(image_files, output_file, progress_callback)


def batch_convert_to_pdf(input_files, output_folder=None, progress_callback=None):
    """Convert multiple images to PDF."""
    return image2pdf_engine.batch_convert_to_pdf(input_files, output_folder, progress_callback)


def get_supported_formats():
    return image2pdf_engine.supported_formats()


def get_engine_info():
    return {
        "name": image2pdf_engine.get_engine_name(),
        "supported_formats": image2pdf_engine.supported_formats(),
        "pillow_installed": image2pdf_engine.is_pillow_installed()
    }


# ============================================================
# MAIN
# ============================================================

def main():
    import argparse
    
    parser = argparse.ArgumentParser(
        description="Convert images to PDF using Pillow",
        epilog="Supported formats: .jpg, .jpeg, .png, .bmp, .gif, .tiff, .tif, .webp"
    )
    parser.add_argument(
        "input",
        nargs="?",
        help="Input image path"
    )
    parser.add_argument(
        "-o", "--output",
        help="Output PDF file path (optional)"
    )
    parser.add_argument(
        "--gui",
        action="store_true",
        help="Open GUI application"
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
    parser.add_argument(
        "--merge",
        action="store_true",
        help="Merge all images into a single PDF"
    )
    
    args = parser.parse_args()
    
    # AfiÃˆâ„¢eazÃ„Æ’ informaÃˆâ€ºii
    if args.info:
        info = get_engine_info()
        print("=" * 50)
        print("FormatForge - Image to PDF Converter")
        print("=" * 50)
        print(f"Engine: {info['name']}")
        print(f"Pillow installed: {'Ã¢Å“â€¦' if info['pillow_installed'] else 'Ã¢ÂÅ’'}")
        print(f"Supported formats:")
        for fmt in info['supported_formats']:
            print(f"  {fmt}")
        print("=" * 50)
        
        if not info['pillow_installed']:
            print("\nÃ¢Å¡Â Ã¯Â¸Â  Pillow not installed!")
            print("   Run: pip install Pillow")
        
        return
    
    # Batch conversion
    if args.batch:
        if args.merge:
            # Merge all images into one PDF
            if args.output_folder:
                output_file = Path(args.output_folder) / "merged.pdf"
            else:
                output_file = None
            
            result = merge_images_to_pdf(args.batch, str(output_file) if output_file else None)
            print(f"Ã¢Å“â€¦ Success! Output: {result}")
            return
        
        if not args.output_folder:
            print("Error: --output-folder is required for batch conversion")
            sys.exit(1)
        
        results = batch_convert_to_pdf(args.batch, args.output_folder)
        success = [r for r in results if r is not None]
        failed = len(results) - len(success)
        
        print(f"\nÃ¢Å“â€¦ Success: {len(success)}")
        print(f"Ã¢ÂÅ’ Failed: {failed}")
        return
    
    # Mod GUI
    if args.gui or (not args.input and not args.output):
        app = ImageConverterApp()
        app.run()
        return
    
    # Mod linie de comandÃ„Æ’ - single file
    if not args.input:
        parser.print_help()
        return
    
    if not Path(args.input).exists():
        print(f"Error: Input file not found: {args.input}")
        sys.exit(1)
    
    try:
        result = convert_to_pdf(args.input, args.output)
        print(f"Ã¢Å“â€¦ Success! Output: {result}")
    except Exception as e:
        print(f"Ã¢ÂÅ’ Error: {e}")
        sys.exit(1)


if __name__ == "__main__":
    main()
