"""Convierte docs/02-LLD.md a Word (.docx) en docs/entregables/.

El HLD ya no se genera aquí: desde la v1.2 el entregable docs/entregables/HLD-Terrun.docx
tiene el diseño visual de Terrun y se edita en Word (docs/01-HLD.md es su versión de lectura).

Uso:
  uv run --native-tls --with python-docx python scripts/generar_documentos.py
  soffice --headless --convert-to pdf --outdir docs/entregables docs/entregables/*.docx
"""
import re
from pathlib import Path

from docx import Document
from docx.enum.table import WD_TABLE_ALIGNMENT
from docx.enum.text import WD_ALIGN_PARAGRAPH, WD_BREAK
from docx.oxml import OxmlElement
from docx.oxml.ns import qn
from docx.shared import Cm, Pt, RGBColor

RAIZ = Path(__file__).resolve().parent.parent
DOCS = RAIZ / "docs"
SALIDA = DOCS / "entregables"
AZUL = RGBColor(0x1F, 0x4E, 0x79)
GRIS = RGBColor(0x55, 0x55, 0x55)
ANCHO_UTIL_CM = 17.0  # carta con márgenes de 2.3 cm

DOCUMENTOS = [
    ("02-LLD.md", "LLD-Terrun.docx", "LLD"),
]
EQUIPO = "Ashley · Diana · Jorge · Rowlis · Rangelis · Jaime"


# --- utilidades XML -----------------------------------------------------------
def sombrear(celda, color_hex):
    tcPr = celda._tc.get_or_add_tcPr()
    shd = OxmlElement("w:shd")
    shd.set(qn("w:val"), "clear")
    shd.set(qn("w:color"), "auto")
    shd.set(qn("w:fill"), color_hex)
    tcPr.append(shd)


def sombrear_parrafo(p, color_hex):
    pPr = p._p.get_or_add_pPr()
    shd = OxmlElement("w:shd")
    shd.set(qn("w:val"), "clear")
    shd.set(qn("w:color"), "auto")
    shd.set(qn("w:fill"), color_hex)
    pPr.append(shd)


def campo(parrafo, instruccion):
    run = parrafo.add_run()
    for tipo, texto in (("begin", None), (None, instruccion), ("separate", None), (None, "1"), ("end", None)):
        if tipo:
            el = OxmlElement("w:fldChar")
            el.set(qn("w:fldCharType"), tipo)
        elif texto == instruccion:
            el = OxmlElement("w:instrText")
            el.set(qn("xml:space"), "preserve")
            el.text = texto
        else:
            el = OxmlElement("w:t")
            el.text = texto
        run._r.append(el)


def repetir_encabezado(fila):
    trPr = fila._tr.get_or_add_trPr()
    el = OxmlElement("w:tblHeader")
    el.set(qn("w:val"), "true")
    trPr.append(el)


# --- texto en línea ------------------------------------------------------------
PATRON = re.compile(r"(\*\*[^*]+\*\*|`[^`]+`|\*[^*]+\*)")


def texto_en_linea(parrafo, texto, tam=None, color=None, negrita=False):
    texto = texto.replace("\\|", "|")
    for parte in PATRON.split(texto):
        if not parte:
            continue
        if parte.startswith("**") and parte.endswith("**"):
            run = parrafo.add_run(parte[2:-2])
            run.bold = True
        elif parte.startswith("`") and parte.endswith("`"):
            run = parrafo.add_run(parte[1:-1])
            run.font.name = "Consolas"
            run.font.color.rgb = RGBColor(0x8B, 0x1A, 0x1A)
        elif parte.startswith("*") and parte.endswith("*") and len(parte) > 2:
            run = parrafo.add_run(parte[1:-1])
            run.italic = True
        else:
            run = parrafo.add_run(parte)
        if tam:
            run.font.size = Pt(tam)
        if color and not run.font.color.rgb:
            run.font.color.rgb = color
        if negrita:
            run.bold = True


# --- bloques ------------------------------------------------------------------
def dividir_fila(linea):
    celdas = re.split(r"(?<!\\)\|", linea.strip())
    return [c.strip() for c in celdas[1:-1]]


def agregar_tabla(doc, filas):
    encabezado, cuerpo = filas[0], filas[2:]
    ncol = len(encabezado)
    tabla = doc.add_table(rows=1, cols=ncol)
    tabla.style = "Table Grid"
    tabla.alignment = WD_TABLE_ALIGNMENT.CENTER
    tabla.autofit = False
    # anchos proporcionales al contenido
    validas = [f for k, f in enumerate(filas) if k != 1]
    pesos = []
    for i in range(ncol):
        largos = [len(re.sub(r"[*`]", "", f[i])) if i < len(f) else 0 for f in validas]
        pesos.append(min(max(0.7 * sum(largos) / len(largos) + 0.3 * max(largos), 4), 60))
    # mínimo: que la palabra más larga de la columna quepa sin partirse (tope 4.5 cm)
    minimos = []
    for i in range(ncol):
        palabra = max((len(w) for f in validas if i < len(f)
                       for w in re.sub(r"[*`]", "", f[i]).split()), default=4)
        minimos.append(min(palabra * 0.19 + 0.35, 4.5))
    libre = max(ANCHO_UTIL_CM - sum(minimos), 0)
    total = sum(pesos)
    cms = [m + libre * p / total for m, p in zip(minimos, pesos)]
    escala = ANCHO_UTIL_CM / sum(cms)
    anchos = [Cm(c * escala) for c in cms]
    for i, col in enumerate(tabla.columns):
        col.width = anchos[i]  # w:gridCol (lo que respeta LibreOffice)
    tam = 8 if ncol >= 4 else 9
    for i, texto in enumerate(encabezado):
        c = tabla.rows[0].cells[i]
        c.width = anchos[i]
        c.paragraphs[0].text = ""
        texto_en_linea(c.paragraphs[0], texto, tam=tam, negrita=True, color=RGBColor(0xFF, 0xFF, 0xFF))
        sombrear(c, "1F4E79")
    repetir_encabezado(tabla.rows[0])
    for n, fila in enumerate(cuerpo):
        celdas = tabla.add_row().cells
        for i in range(ncol):
            c = celdas[i]
            c.width = anchos[i]
            texto_en_linea(c.paragraphs[0], fila[i] if i < len(fila) else "", tam=tam)
            if n % 2 == 1:
                sombrear(c, "EEF3F8")
    doc.add_paragraph().paragraph_format.space_after = Pt(2)


def agregar_codigo(doc, lineas):
    for i, l in enumerate(lineas):
        p = doc.add_paragraph()
        pf = p.paragraph_format
        pf.space_before = Pt(0)
        pf.space_after = Pt(0)
        pf.left_indent = Cm(0.3)
        run = p.add_run(l if l else " ")
        run.font.name = "Consolas"
        run._element.rPr.rFonts.set(qn("w:eastAsia"), "Consolas")
        run.font.size = Pt(7.5)
        sombrear_parrafo(p, "F4F6F8")
    doc.add_paragraph().paragraph_format.space_after = Pt(2)


def agregar_imagen(doc, ruta, alt):
    p = doc.add_paragraph()
    p.alignment = WD_ALIGN_PARAGRAPH.CENTER
    p.add_run().add_picture(str(ruta), width=Cm(ANCHO_UTIL_CM))
    cap = doc.add_paragraph()
    cap.alignment = WD_ALIGN_PARAGRAPH.CENTER
    r = cap.add_run(f"Figura: {alt}")
    r.italic = True
    r.font.size = Pt(8.5)
    r.font.color.rgb = GRIS


# --- documento ------------------------------------------------------------------
def configurar(doc, etiqueta, meta):
    sec = doc.sections[0]
    sec.page_width, sec.page_height = Cm(21.59), Cm(27.94)
    for lado in ("left_margin", "right_margin"):
        setattr(sec, lado, Cm(2.3))
    sec.top_margin, sec.bottom_margin = Cm(2.2), Cm(2.0)
    estilos = doc.styles
    normal = estilos["Normal"]
    normal.font.name = "Calibri"
    normal.element.rPr.rFonts.set(qn("w:eastAsia"), "Calibri")
    normal.font.size = Pt(10.5)
    normal.paragraph_format.space_after = Pt(5)
    normal.paragraph_format.line_spacing = 1.12
    for nombre, tam, antes in (("Heading 1", 16, 16), ("Heading 2", 13, 12), ("Heading 3", 11.5, 10)):
        e = estilos[nombre]
        e.font.name = "Calibri"
        e.element.rPr.rFonts.set(qn("w:eastAsia"), "Calibri")
        e.font.size = Pt(tam)
        e.font.bold = True
        e.font.color.rgb = AZUL
        e.paragraph_format.space_before = Pt(antes)
        e.paragraph_format.space_after = Pt(5)
        e.paragraph_format.keep_with_next = True
    # encabezado y pie
    sec.different_first_page_header_footer = True
    hp = sec.header.paragraphs[0]
    hp.alignment = WD_ALIGN_PARAGRAPH.RIGHT
    r = hp.add_run(f"Terrun — {etiqueta} v{meta.get('version', '1.0')}")
    r.font.size = Pt(8.5)
    r.font.color.rgb = GRIS
    fp = sec.footer.paragraphs[0]
    fp.alignment = WD_ALIGN_PARAGRAPH.CENTER
    r = fp.add_run("Página ")
    r.font.size = Pt(8.5)
    campo(fp, "PAGE")


def portada(doc, meta, etiqueta):
    for _ in range(7):
        doc.add_paragraph()
    p = doc.add_paragraph()
    p.alignment = WD_ALIGN_PARAGRAPH.CENTER
    r = p.add_run(meta["titulo"])
    r.bold = True
    r.font.size = Pt(34)
    r.font.color.rgb = AZUL
    p = doc.add_paragraph()
    p.alignment = WD_ALIGN_PARAGRAPH.CENTER
    r = p.add_run("Conquista tu zona · Cada carrera suma territorio")
    r.italic = True
    r.font.size = Pt(13)
    r.font.color.rgb = GRIS
    doc.add_paragraph()
    p = doc.add_paragraph()
    p.alignment = WD_ALIGN_PARAGRAPH.CENTER
    r = p.add_run(meta["subtitulo"])
    r.bold = True
    r.font.size = Pt(18)
    for _ in range(6):
        doc.add_paragraph()
    for linea in (f"Asignatura: {meta['asignatura']}", f"Equipo: {EQUIPO}",
                  f"Versión {meta['version']} · {meta['fecha']}"):
        p = doc.add_paragraph()
        p.alignment = WD_ALIGN_PARAGRAPH.CENTER
        r = p.add_run(linea)
        r.font.size = Pt(11)
    doc.add_paragraph().add_run().add_break(WD_BREAK.PAGE)


def indice(doc, lineas):
    doc.add_heading("Contenido", level=1)
    for l in lineas:
        if l.startswith("# ") and not l.startswith("# Control"):
            p = doc.add_paragraph()
            p.paragraph_format.space_after = Pt(1)
            r = p.add_run(l[2:].strip())
            r.bold = True
            r.font.size = Pt(10)
        elif l.startswith("## "):
            p = doc.add_paragraph()
            p.paragraph_format.left_indent = Cm(0.6)
            p.paragraph_format.space_after = Pt(0)
            r = p.add_run(l[3:].strip())
            r.font.size = Pt(9.5)
    doc.add_paragraph().add_run().add_break(WD_BREAK.PAGE)


def convertir(origen, destino, etiqueta):
    texto = (DOCS / origen).read_text(encoding="utf-8")
    meta = {}
    if texto.startswith("---"):
        _, fm, texto = texto.split("---", 2)
        for l in fm.strip().splitlines():
            k, v = l.split(":", 1)
            meta[k.strip()] = v.strip()
    lineas = texto.strip("\n").splitlines()

    doc = Document()
    configurar(doc, etiqueta, meta)
    portada(doc, meta, etiqueta)

    i = 0
    indice_hecho = False
    while i < len(lineas):
        l = lineas[i]
        if l.startswith("# "):
            if indice_hecho is False and not l.startswith("# Control"):
                indice(doc, lineas)
                indice_hecho = True
            doc.add_heading(l[2:].strip(), level=1)
        elif l.startswith("### "):
            doc.add_heading(l[4:].strip(), level=3)
        elif l.startswith("## "):
            doc.add_heading(l[3:].strip(), level=2)
        elif l.startswith("```"):
            bloque = []
            i += 1
            while i < len(lineas) and not lineas[i].startswith("```"):
                bloque.append(lineas[i])
                i += 1
            agregar_codigo(doc, bloque)
        elif l.startswith("|"):
            filas = []
            while i < len(lineas) and lineas[i].startswith("|"):
                filas.append(dividir_fila(lineas[i]))
                i += 1
            agregar_tabla(doc, filas)
            continue
        elif m := re.match(r"!\[(.*?)\]\((.*?)\)", l):
            agregar_imagen(doc, DOCS / m.group(2), m.group(1))
        elif re.match(r"^\s*[-*] ", l):
            nivel = (len(l) - len(l.lstrip())) // 2
            p = doc.add_paragraph(style="List Bullet" if nivel == 0 else "List Bullet 2")
            p.paragraph_format.space_after = Pt(2)
            texto_en_linea(p, re.sub(r"^\s*[-*] ", "", l))
        elif m := re.match(r"^(\d+)\. (.*)", l):
            p = doc.add_paragraph()
            pf = p.paragraph_format
            pf.left_indent = Cm(0.9)
            pf.first_line_indent = Cm(-0.5)
            pf.space_after = Pt(2)
            p.add_run(f"{m.group(1)}.  ")
            texto_en_linea(p, m.group(2))
        elif l.strip():
            p = doc.add_paragraph()
            texto_en_linea(p, l)
        i += 1

    SALIDA.mkdir(parents=True, exist_ok=True)
    doc.save(SALIDA / destino)
    print("ok", destino)


if __name__ == "__main__":
    for origen, destino, etiqueta in DOCUMENTOS:
        convertir(origen, destino, etiqueta)
