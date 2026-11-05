"""Rebuild the approved Phase 6 ScriptableObject assets; preserves existing GUIDs."""
from pathlib import Path
import re, uuid, json, shutil, unicodedata
ROOT=Path(__file__).resolve().parents[1]
def meta(path):
    p=Path(str(path)+'.meta')
    if not p.exists():p.write_text('fileFormatVersion: 2\nguid: '+uuid.uuid4().hex+'\n')
    return re.search(r'guid: (\w+)',p.read_text()).group(1)
def clean(text):return ''.join(c for c in unicodedata.normalize('NFKD',text) if not unicodedata.combining(c)).upper()
def icon(id,source):
    dest=ROOT/'Assets/_Placeholders/Phase6'/('PH_'+id+'.png');dest.parent.mkdir(parents=True,exist_ok=True)
    src=ROOT/'Assets/Resources/TablePresentation/Icons'/(source+'.png');shutil.copyfile(src,dest)
    guid=meta(dest)
    template=Path(str(src)+'.meta').read_text();template=re.sub(r'guid: \w+','guid: '+guid,template,count=1)
    Path(str(dest)+'.meta').write_text(template)
    return '{fileID: 21300000, guid: '+guid+', type: 3}'
def ref(path):return '{fileID: 11400000, guid: '+meta(path)+', type: 2}'
def asset(folder,id,cls,fields):
    path=ROOT/'Assets/Resources'/folder/(id+'.asset');path.parent.mkdir(parents=True,exist_ok=True)
    script=meta(ROOT/'Assets/_Game/Runtime/Scoring'/(cls+'.cs'))
    head='%YAML 1.1\n%TAG !u! tag:unity3d.com,2011:\n--- !u!114 &11400000\nMonoBehaviour:\n'
    head+='  m_ObjectHideFlags: 0\n  m_CorrespondingSourceObject: {fileID: 0}\n  m_PrefabInstance: {fileID: 0}\n  m_PrefabAsset: {fileID: 0}\n  m_GameObject: {fileID: 0}\n  m_Enabled: 1\n  m_EditorHideFlags: 0\n'
    head+='  m_Script: {fileID: 11500000, guid: '+script+', type: 3}\n  m_Name: '+id+'\n  m_EditorClassIdentifier: DoubleDown.Runtime::DoubleDown.Scoring.'+cls+'\n'
    for k,v in dict(id=id,**fields).items():
        val=v if isinstance(v,str) and v.startswith('{fileID:') else json.dumps(v,ensure_ascii=True)
        head+='  '+k+': '+val+'\n'
    path.write_text(head);meta(path);return path
proposal=(ROOT/'docs/PHASE_6_CONTENT_PROPOSAL.md').read_text(encoding='utf-8')
rows={}
for line in proposal.splitlines():
    if line.startswith('| '):
        cells=[x.strip() for x in line.strip('|').split('|')]
        if len(cells)==3:rows[cells[0]]=cells[1:]
triggers=['Victory','Aces','Faces','Suit','Push','TwentyOne','StandVictory','DoubleVictory','SplitVictory','ExtraCards','Streak','ScoredHand','Natural','FiveCards','Soft','Hard','FourSuits','SameSuit','OpeningPair','Even','Odd','Low','High','Nineteen','Twenty','Eighteen','RedHand','BlackHand','LongHand']
# trigger, operation (base/add mult/factor), amount, suit
relics=[('Suit',1,.3,2),('Suit',1,.3,1),('Suit',1,.3,0),('Suit',1,.3,3),('Aces',1,.8,0),('Faces',1,.4,0),('DoubleVictory',0,24,0),('SplitVictory',1,1,0),('Push',0,18,0),('StandVictory',0,20,0),('ExtraCards',1,.4,0),('Streak',0,4,0),('TwentyOne',0,30,0),('Natural',2,1.5,0),('FiveCards',2,1.8,0),('Soft',1,1,0),('Hard',0,15,0),('FourSuits',2,1.6,0),('SameSuit',2,1.4,0),('OpeningPair',1,1,0),('Even',0,4,0),('Odd',0,4,0),('Low',1,.25,0),('High',1,.25,0),('Nineteen',2,1.5,0),('Twenty',2,1.35,0),('Eighteen',1,2,0),('RedHand',2,1.4,0),('BlackHand',2,1.4,0),('Push',2,1.2,0),('DoubleVictory',2,1.6,0),('SplitVictory',2,1.4,0),('LongHand',2,1.4,0),('Victory',1,2,0),('ScoredHand',2,2,0)]
icons=['suit_hearts','suit_diamonds','suit_clubs','suit_spades','card','crown_a','dice_2','cards_diagonal','tokens','card_tap','cards_stack','tag_infinite','card_target','crown_b','cards_stack','card','card','cards_diagonal','suit_hearts','cards_flip','dice_2','card','card','card','token','tokens','card_target','suit_hearts','suit_spades','cards_flip','dice_2','cards_diagonal','tag_infinite','crown_a','crown_b']
for i,(trigger,operation,amount,suit) in enumerate(relics,15):
    name,description=rows['R'+str(i)];desc,rare=description.split('. Rareza: ')
    rarity=['Común','Rara','Épica','Legendaria'].index(rare)
    # Full effect text is retained in the asset, shown in the collection inspector.
    asset('Relics',str(i),'RelicDefinition',dict(displayName=clean(name),description=clean(desc),icon=icon('R'+str(i),icons[i-15]),trigger=triggers.index(trigger),operation=operation,amount=amount,suit=suit,rarity=rarity))
mods=[('E01',0,dict(chips=12,visualEdition=1)),('E02',0,dict(multiplier=.6,visualEdition=2)),('E03',0,dict(multiplierFactor=1.15,visualEdition=3)),('M01',1,dict(chips=20)),('M02',1,dict(multiplier=1)),('M03',1,dict(wild=1)),('M04',1,dict(gold=2)),('M05',1,dict(multiplierFactor=1.5,destroyAfterScoring=1)),('M06',1,dict(chips=45,destroyAfterScoring=1)),('S01',2,dict(repeatBonuses=1)),('S02',2,dict(gold=1)),('S03',2,dict(multiplierPerPosition=.2))]
paths={}
for id,layer,extra in mods:
    name,desc=rows[id];fields=dict(displayName=clean(name),description=clean(desc),layer=layer,icon=icon(id,['crown_a','card_target','token'][layer]),chips=0,multiplier=0,multiplierFactor=1,gold=0,wild=0,destroyAfterScoring=0,repeatBonuses=0,multiplierPerPosition=0,visualEdition=0);fields.update(extra)
    paths[id]=asset('CardModifications',id,'CardModificationDefinition',fields)
for i in range(1,21):
    id=f'C{i:02}';name,desc=rows[id]
    effect=i-1 if i<=4 else 4 if i<=8 else 5
    fields=dict(displayName=clean(name),description=clean(desc),icon=icon(id,'cards_flip' if i<=8 else 'card_target'),effect=effect,suit={5:2,6:1,7:0,8:3}.get(i,0),modification='{fileID: 0}')
    if i>=9:
        modifier_id=mods[i-9][0]
        fields['modification']=ref(paths[modifier_id])
        fields['description']=clean(rows[modifier_id][1])+'. REEMPLAZA SU MISMA CAPA.'
        if modifier_id in ('M05','M06'):fields['description']+=' CON 24 CARTAS SE CONSUME SOLO LA MEJORA.'
    asset('Consumables',id,'ConsumableDefinition',fields)
for i,(effect,amount,pre) in enumerate([(0,1,''),(0,1,'V01'),(1,1,''),(1,1,'V03'),(2,1,''),(2,1,'V05'),(3,1,''),(4,1,''),(5,1,''),(6,5,''),(7,2,''),(8,20,'')],1):
    id=f'V{i:02}';name,desc=rows[id]
    asset('Vouchers',id,'VoucherDefinition',dict(displayName=clean(name),description=clean(desc),prerequisite=pre,effect=effect,amount=amount,icon=icon(id,'tokens')))
for i in range(4,11):
    id=f'B{i:02}';name,desc=rows[id];fields=dict(displayName=clean(name),description=clean(desc),icon=icon(id,'crown_b'),playerAcesLow=0,dealerStandTotal=19 if i==10 else 17,maximumHands=0,firstTable=4 if i==10 else 3 if i in (6,7) else 2,disableDouble=int(i==8),disableSplit=int(i==9),debuffedSuit={4:2,5:1,6:0,7:3}.get(i,-1),goalFactor=1.5 if i==10 else 1)
    asset('Bosses',id,'BossDefinition',fields)
for p in (ROOT/'Assets/_Game').rglob('*.cs'):meta(p)
for folder in ['Assets/Resources/CardModifications','Assets/Resources/Consumables','Assets/Resources/Vouchers','Assets/_Placeholders/Phase6']:
    meta(ROOT/folder)
(ROOT/'Assets/_Placeholders/Phase6/README.md').write_text('Provisional Phase 6 symbols approved by the user. Copies of the supplied Kenney board-game icons (CC0), already imported in Resources/TablePresentation/Icons. PH_ filenames correspond to docs/PHASE_6_CONTENT_PROPOSAL.md. Replace images keeping .meta GUIDs. These are not final item artwork.\n')
print('Phase 6 catalog: 50 relics / 12 modifications / 20 consumables / 12 vouchers / 10 bosses.')
