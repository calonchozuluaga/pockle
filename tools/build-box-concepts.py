"""Create editable concept carton nets and a PDF. Requires reportlab and PyMuPDF.
These are digital packaging studies, not supplier-approved manufacturing dielines.
"""
from pathlib import Path
import math
import html
from reportlab.pdfgen import canvas
from reportlab.lib.colors import HexColor
import fitz
from PIL import Image, ImageDraw

ROOT = Path(__file__).resolve().parents[1]
OUT = ROOT / 'docs' / 'packaging'
MM = 72 / 25.4
WIDTH, HEIGHT = 330, 278
CUT = [(27,90),(97,90),(102,60),(162,60),(167,90),(167,20),
       (172,20),(174,8),(230,8),(232,20),(237,20),(237,90),
       (242,60),(302,60),(307,90),(307,160),(302,190),(242,190),(237,160),
       (167,160),(162,190),(102,190),(97,160),(97,230),(92,230),
       (90,242),(34,242),(32,230),(27,230),(27,160),(15,154),(15,96)]
FOLDS = [(x,90,x,160) for x in [27,97,167,237]] + [
    (97,90,167,90),(167,90,237,90),(237,90,307,90),(167,20,237,20),
    (27,160,97,160),(97,160,167,160),(237,160,307,160),(27,230,97,230)]
THEMES = [
    dict(slug='jelly-garden',name='JELLY GARDEN',kind='JELLY COLLECTION',base='#F6BCA0',panel='#FFDCC3',
         ink='#523148',accent='#D5657B',motif='drop',descriptor='SOFT / SQUISHY',subtitle='Peach, coral and honey tones'),
    dict(slug='midnight-glow',name='MIDNIGHT GLOW',kind='GLOW COLLECTION',base='#242A4A',panel='#343958',
         ink='#F4F5DB',accent='#CBEAA3',motif='moon',descriptor='LIGHTS OFF / GLOW ON',subtitle='Indigo with mint and luminous lime'),
    dict(slug='gold-confetti',name='GOLD CONFETTI',kind='CONFETTI COLLECTION',base='#F7ECDD',panel='#E9DDEB',
         ink='#5D425F',accent='#B9852E',motif='star',descriptor='SPARKLE / SQUISH',subtitle='Warm ivory, lavender and gold stars'),
]

class Sheet:
    def __init__(self,pdf):
        self.pdf=pdf; self.svg=[]
    def polygon(self,points,fill,stroke=None,width=.3,dash=None):
        pts=' '.join(f'{x:g},{y:g}' for x,y in points)
        attrs=f'fill="{fill or "none"}"'
        if stroke: attrs+=f' stroke="{stroke}" stroke-width="{width}"'
        if dash: attrs+=' stroke-dasharray="2 1.5"'
        self.svg.append(f'<polygon points="{pts}" {attrs}/>')
        path=self.pdf.beginPath();path.moveTo(points[0][0]*MM,(HEIGHT-points[0][1])*MM)
        for x,y in points[1:]:path.lineTo(x*MM,(HEIGHT-y)*MM)
        path.close()
        self.pdf.setLineWidth(width*MM);self.pdf.setDash([2*MM,1.5*MM] if dash else [])
        if fill:self.pdf.setFillColor(HexColor(fill))
        if stroke:self.pdf.setStrokeColor(HexColor(stroke))
        self.pdf.drawPath(path,fill=bool(fill),stroke=bool(stroke))
    def rect(self,x,y,w,h,fill):self.polygon([(x,y),(x+w,y),(x+w,y+h),(x,y+h)],fill)
    def line(self,x1,y1,x2,y2,color,width=.3,dash=False):
        self.svg.append(f'<line x1="{x1}" y1="{y1}" x2="{x2}" y2="{y2}" stroke="{color}" stroke-width="{width}"'+(' stroke-dasharray="2 1.5"' if dash else '')+'/>')
        self.pdf.setStrokeColor(HexColor(color));self.pdf.setLineWidth(width*MM);self.pdf.setDash([2*MM,1.5*MM] if dash else [])
        self.pdf.line(x1*MM,(HEIGHT-y1)*MM,x2*MM,(HEIGHT-y2)*MM)
    def circle(self,x,y,r,color):
        self.svg.append(f'<circle cx="{x}" cy="{y}" r="{r}" fill="{color}"/>')
        self.pdf.setFillColor(HexColor(color));self.pdf.circle(x*MM,(HEIGHT-y)*MM,r*MM,stroke=0,fill=1)
    def text(self,x,y,value,size,color,center=True,bold=False):
        self.svg.append(f'<text x="{x}" y="{y}" font-family="Helvetica,Arial,sans-serif" font-size="{size}" fill="{color}"'+(' text-anchor="middle"' if center else '')+(' font-weight="bold"' if bold else '')+f'>{html.escape(value)}</text>')
        self.pdf.setFillColor(HexColor(color));self.pdf.setFont('Helvetica-Bold' if bold else 'Helvetica',size*MM)
        if center:self.pdf.drawCentredString(x*MM,(HEIGHT-y)*MM,value)
        else:self.pdf.drawString(x*MM,(HEIGHT-y)*MM,value)
    def motif(self,x,y,size,theme):
        color=theme['accent']
        if theme['motif']=='star':
            pts=[]
            for i in range(10):
                radius=size if i%2==0 else size*.43
                a=-math.pi/2+i*math.pi/5
                pts.append((x+math.cos(a)*radius,y+math.sin(a)*radius))
            self.polygon(pts,color)
        elif theme['motif']=='moon':
            self.circle(x,y,size,color);self.circle(x+size*.42,y-size*.30,size*.80,theme['base'])
        else:
            # Abstract jelly droplet, a collection symbol rather than a revealed variant.
            self.circle(x,y+size*.24,size*.70,color)
            self.polygon([(x-size*.66,y+size*.10),(x,y-size),(x+size*.66,y+size*.10)],color)
    def save(self,path):
        path.write_text(f'<svg xmlns="http://www.w3.org/2000/svg" width="{WIDTH}mm" height="{HEIGHT}mm" viewBox="0 0 {WIDTH} {HEIGHT}">\n'+ '\n'.join(self.svg)+'\n</svg>\n')


def main():
    OUT.mkdir(parents=True,exist_ok=True)
    pdf=canvas.Canvas(str(OUT/'pockle-box-dielines.pdf'),pagesize=(WIDTH*MM,HEIGHT*MM))
    pdf.setTitle('Pockle collection box concepts — illustrative dielines')
    for index,theme in enumerate(THEMES):
        sheet=Sheet(pdf);sheet.rect(0,0,WIDTH,HEIGHT,'#FBF8F2')
        sheet.polygon(CUT,theme['base'])
        for x in [97,237]:sheet.rect(x,90,70,70,theme['panel'])
        sheet.text(62,104,'Pockle',8.4,theme['ink'],bold=True)
        sheet.text(62,112,theme['name'],3.35,theme['ink'],bold=True)
        sheet.motif(62,131,11,theme)
        sheet.text(62,152,theme['kind'],2.65,theme['ink'],bold=True)
        sheet.text(132,105,theme['kind'],3,theme['ink'],bold=True)
        for i,(dx,dy) in enumerate([(-16,-7),(0,0),(16,8)]):sheet.motif(132+dx,127+dy,4.3,theme)
        sheet.text(132,149,theme['descriptor'],2.45,theme['ink'])
        sheet.text(202,104,'Pockle',7,theme['ink'],bold=True)
        sheet.text(202,118,'A LITTLE WONDER',3.5,theme['ink'],bold=True)
        sheet.text(202,126,'One surprise Pockle',3,theme['ink'])
        sheet.text(202,132,'from this collection.',3,theme['ink'])
        sheet.text(202,149,'COLLECT / OPEN / SQUISH',2.5,theme['ink'],bold=True)
        sheet.text(272,107,theme['name'],3.35,theme['ink'],bold=True)
        sheet.motif(272,126,8,theme)
        sheet.text(272,149,'SURPRISE VARIANT',2.65,theme['ink'],bold=True)
        sheet.text(202,58,'Pockle',8,theme['ink'],bold=True)
        sheet.text(202,67,theme['kind'],3,theme['ink'],bold=True)
        sheet.motif(62,195,8,theme)
        sheet.text(62,214,'A little wonder, in your hands.',2.8,theme['ink'])
        # Separate technical overlays from the collection art for easy vector editing.
        sheet.svg.append('<g id="cut-line">');sheet.polygon(CUT,None,'#D94470',.35);sheet.svg.append('</g>')
        sheet.svg.append('<g id="fold-lines">')
        for line in FOLDS:sheet.line(*line,'#1687AD',.3,True)
        sheet.svg.append('</g>')
        sheet.text(132,47,'SIDE DUST FLAP',2.5,'#77716D')
        sheet.text(272,47,'SIDE DUST FLAP',2.5,'#77716D')
        sheet.text(62,78,'FRONT',2.6,'#77716D',bold=True)
        sheet.text(202,177,'BACK',2.6,'#77716D',bold=True)
        sheet.text(18,253,f'0{index+1} / {theme["name"]}',5,'#523148',center=False,bold=True)
        sheet.text(18,261,'Illustrative 70 mm cube carton / reverse tuck ends / 12 mm glue tab',3,'#77716D',center=False)
        sheet.line(18,269,28,269,'#D94470',.45);sheet.text(31,270,'Cut',3,'#77716D',center=False)
        sheet.line(52,269,62,269,'#1687AD',.4,True);sheet.text(65,270,'Fold',3,'#77716D',center=False)
        sheet.text(108,270,'CONCEPT ONLY / no bleed, stock, closure tolerances or print approval',2.65,'#77716D',center=False)
        sheet.save(OUT/(theme['slug']+'-dieline.svg'));pdf.showPage()
    pdf.save()
    doc=fitz.open(OUT/'pockle-box-dielines.pdf')
    thumbs=[]
    for i,page in enumerate(doc):
        pix=page.get_pixmap(matrix=fitz.Matrix(1.4,1.4),alpha=False)
        path=OUT/(THEMES[i]['slug']+'-dieline.png');pix.save(path)
        img=Image.open(path);img.thumbnail((700,610));thumbs.append(img.copy())
    board=Image.new('RGB',(2140,700),'#FBF8F2');draw=ImageDraw.Draw(board)
    draw.text((30,18),'POCKLE / COLLECTION BOX DIELINE STUDIES',fill='#523148')
    for i,img in enumerate(thumbs):board.paste(img,(20+i*710,65))
    board.save(OUT/'pockle-dieline-overview.png')
    print('Created 3 SVG dielines, 3-page PDF, per-sheet PNGs and overview.')

if __name__=='__main__':main()
