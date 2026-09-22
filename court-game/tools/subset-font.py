"""Rebuild the OFL font subset from all game text (fonttools==4.60.1).

Run before Unity build whenever adding text. This static prototype has no AI or
user text. For future dynamic dialogue, ship an appropriate full/localized font
instead of assuming this subset contains arbitrary Chinese characters.
"""
from pathlib import Path
import re
import sys
from fontTools import subset
from fontTools.ttLib import TTFont

root = Path(__file__).resolve().parents[1]
source = root / 'tools/fonts/NotoSansCJKtc-Regular.otf'
output = root / 'Assets/UI/NotoSansCJKtc-Regular.otf'
characters = set(range(32, 127)) | set(range(0x3000, 0x3040))
for path in (root / 'Assets').rglob('*'):
    if path.suffix not in {'.cs', '.unity', '.prefab', '.asset'}:
        continue
    text = path.read_text(encoding='utf-8-sig')
    characters.update(map(ord, text))
    characters.update(int(c, 16) for c in re.findall(r'\\u([0-9a-fA-F]{4})', text))
if '--check' in sys.argv:
    original = TTFont(source).getBestCmap()
    actual = TTFont(output).getBestCmap()
    missing = (characters & original.keys()) - actual.keys()
    if missing:
        raise SystemExit('Font subset missing characters; run subset-font.py: ' + str(sorted(missing)))
    print('COURT_FONT_COVERAGE_PASSED')
    raise SystemExit(0)
font = TTFont(source)
options = subset.Options()
options.name_IDs = ['*']
options.name_languages = ['*']
options.notdef_glyph = True
options.notdef_outline = True
subsetter = subset.Subsetter(options=options)
subsetter.populate(unicodes=characters)
subsetter.subset(font)
# Distinguish the modified font; retain all copyright/license records.
for record in font['name'].names:
    if record.nameID in {1, 3, 4, 6, 16}:
        record.string = 'EduAI Court Subset'.replace(' ', '' if record.nameID == 6 else ' ').encode(record.getEncoding())
font.save(output)
print(f'Font: {source.stat().st_size:,} -> {output.stat().st_size:,} bytes')
