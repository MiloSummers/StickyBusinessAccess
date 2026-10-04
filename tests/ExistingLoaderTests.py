"""Private file-copy tests. Never run against a player's live game directory."""
from pathlib import Path
import sys,shutil,subprocess,hashlib,json
package,original,scratch=map(lambda s:Path(s).resolve(),sys.argv[1:4])
support=package/'support' if (package/'support/setup.ps1').exists() else package
assert not scratch.exists(),'Choose a new scratch folder'
scratch.mkdir()
game=scratch/'Game with existing BepInEx'
names=['StickyBusiness.exe','UnityPlayer.dll','GameAssembly.dll','StickyBusiness_Data/data.unity3d','StickyBusiness_Data/il2cpp_data/Metadata/global-metadata.dat']
for name in names:
    target=game/name;target.parent.mkdir(parents=True,exist_ok=True);shutil.copy2(original/name,target)
for f in (support/'payload').rglob('*'):
    if not f.is_file():continue
    rel=f.relative_to(support/'payload')
    if str(rel).replace('\\','/').startswith('BepInEx/plugins/'):continue
    target=game/rel;target.parent.mkdir(parents=True,exist_ok=True);shutil.copy2(f,target)
(game/'BepInEx/config/BepInEx.cfg').write_text('[Logging.Console]\nEnabled = true\n',encoding='utf8')
other=game/'BepInEx/plugins/OtherMod/OtherMod.dll';other.parent.mkdir(parents=True);other.write_bytes(b'unrelated mod fixture')
(other.parent/'preferences.cfg').write_text('user preferences',encoding='utf8')
(game/'BepInEx/patchers/EmptyOtherPatcher').mkdir(parents=True)
def snapshot():return {f.relative_to(game).as_posix():hashlib.sha256(f.read_bytes()).hexdigest() for f in game.rglob('*') if f.is_file()}
def batch(name,input=b'\r\n'):
    cmd=f'cmd.exe /d /s /c ""{package/name}" -GameDir "{game}""'
    result=subprocess.run(cmd,input=input,stdout=subprocess.PIPE,stderr=subprocess.STDOUT,timeout=90)
    text=result.stdout.decode(errors='replace');(scratch/(name+'.last.log')).write_text(text)
    return result.returncode,text
def check(condition,label):
    assert condition,label
    print('PASS:',label,flush=True)
before=snapshot()
code,text=batch('Setup.bat',b'N\r\n\r\n')
check(code==0 and 'Declined.' in text and snapshot()==before,'Decline makes no game-folder changes')
code,text=batch('Setup.bat',b'\r\n\r\n')
check(code==0 and 'Declined.' in text and snapshot()==before,'Enter defaults to decline')
code,text=batch('Setup.bat',b'Y\r\n\r\n')
check(code==0 and 'Setup complete.' in text,'Accept installs successfully through Setup.bat')
after=snapshot()
check(all(after.get(n)==h for n,h in before.items()),'Accept preserves every pre-existing file')
state=json.loads((game/'StickyBusinessAccess-install.json').read_text(encoding='utf-8-sig'))
check(state['loaderOwnership']=='external' and len(state['files'])==3 and all(e['path'].startswith('BepInEx\\plugins\\StickyBusinessAccess\\') for e in state['files']),'Receipt owns only three Sticky Access files')
settings=game/'BepInEx/config/local.stickybusiness.access.cfg';settings.write_text('[Accessibility]\nDayLength = 2\n',encoding='utf8')
code,text=batch('Setup.bat')
check(code==0 and 'Updating the existing mod' in text and settings.read_text()=='[Accessibility]\nDayLength = 2\n','Shared-loader update preserves preferences')
check(all(snapshot().get(n)==h for n,h in before.items()),'Shared-loader update preserves loader and other mod files')
code,text=batch('Uninstall.bat')
check(code==0,'Shared-loader uninstall completes')
check(all(snapshot().get(n)==h for n,h in before.items()) and (game/'BepInEx/patchers/EmptyOtherPatcher').is_dir(),'Uninstall preserves external loader, other mods, and their empty directories')
check(not (game/'BepInEx/plugins/StickyBusinessAccess/StickyBusinessAccess.dll').exists(),'Uninstall removes Sticky Access plugin')
code,text=batch('Setup.bat',b'Y\r\n\r\n')
check(code==0 and 'Use the existing loader?' in text,'Shared-loader reinstall asks permission again and succeeds')
check(all(snapshot().get(n)==h for n,h in before.items()),'Reinstall preserves all external files')
code,text=batch('Uninstall.bat');check(code==0,'Second uninstall completes')
ini=game/'doorstop_config.ini';data=ini.read_bytes();ini.write_bytes(data.replace(b'enabled = true',b'enabled = false'))
disabled=snapshot();code,text=batch('Setup.bat',b'Y\r\n\r\n')
check(code==1 and 'disabled' in text and snapshot()==disabled,'Accept cannot bypass disabled loader; no files change')
ini.write_bytes(data)
runtime=game/'dotnet/coreclr.dll';data=runtime.read_bytes();runtime.write_bytes(b'modified runtime fixture')
changed=snapshot();code,text=batch('Setup.bat',b'Y\r\n\r\n')
check(code==1 and 'coreclr.dll' in text and snapshot()==changed,'Accept cannot bypass modified runtime; no files change')
runtime.write_bytes(data)
core=game/'BepInEx/core/BepInEx.Unity.IL2CPP.dll';data=core.read_bytes();core.unlink()
missing=snapshot();code,text=batch('Setup.bat',b'Y\r\n\r\n')
check(code==1 and 'BepInEx 5 and Mono' in text and snapshot()==missing,'Wrong loader family is explained and protected')
core.write_bytes(data)
print('Existing-loader prompt, ownership, update, uninstall, reinstall, and incompatibility tests complete.')
