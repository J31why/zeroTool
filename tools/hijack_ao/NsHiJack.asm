
; dxgi.dll Hijack Project
; Caution: 
;   This project code is for testing purposes only! Please do not use it in any other way.
; Code By : Baymax Patch toOls 
IFDEF HIJACK
LoadLibraryA PROTO
GetSystemDirectoryA PROTO
GetProcAddress PROTO
GetModuleFileNameA PROTO

.data
szDllName db '\dxgi.dll',0

szApplyCompatResolutionQuirking db 'ApplyCompatResolutionQuirking',0
szCompatString db 'CompatString',0
szCompatValue db 'CompatValue',0
szCreateDXGIFactory db 'CreateDXGIFactory',0
szCreateDXGIFactory1 db 'CreateDXGIFactory1',0
szCreateDXGIFactory2 db 'CreateDXGIFactory2',0
szDXGID3D10CreateDevice db 'DXGID3D10CreateDevice',0
szDXGID3D10CreateLayeredDevice db 'DXGID3D10CreateLayeredDevice',0
szDXGID3D10GetLayeredDeviceSize db 'DXGID3D10GetLayeredDeviceSize',0
szDXGID3D10RegisterLayers db 'DXGID3D10RegisterLayers',0
szDXGIDeclareAdapterRemovalSupport db 'DXGIDeclareAdapterRemovalSupport',0
szDXGIDumpJournal db 'DXGIDumpJournal',0
szDXGIGetDebugInterface1 db 'DXGIGetDebugInterface1',0
szDXGIReportAdapterConfiguration db 'DXGIReportAdapterConfiguration',0
szPIXBeginCapture db 'PIXBeginCapture',0
szPIXEndCapture db 'PIXEndCapture',0
szPIXGetCaptureState db 'PIXGetCaptureState',0
szSetAppCompatStringPointer db 'SetAppCompatStringPointer',0
szUpdateHMDEmulationStatus db 'UpdateHMDEmulationStatus',0

pfApplyCompatResolutionQuirking dq 0
pfCompatString dq 0
pfCompatValue dq 0
pfCreateDXGIFactory dq 0
pfCreateDXGIFactory1 dq 0
pfCreateDXGIFactory2 dq 0
pfDXGID3D10CreateDevice dq 0
pfDXGID3D10CreateLayeredDevice dq 0
pfDXGID3D10GetLayeredDeviceSize dq 0
pfDXGID3D10RegisterLayers dq 0
pfDXGIDeclareAdapterRemovalSupport dq 0
pfDXGIDumpJournal dq 0
pfDXGIGetDebugInterface1 dq 0
pfDXGIReportAdapterConfiguration dq 0
pfPIXBeginCapture dq 0
pfPIXEndCapture dq 0
pfPIXGetCaptureState dq 0
pfSetAppCompatStringPointer dq 0
pfUpdateHMDEmulationStatus dq 0

.code

NsStringCatA proc
  push rsi
  push rdi
  xor rsi,rsi
  dec rsi
  xor rdi,rdi
  dec rdi
  xor rax,rax
  dec rdx

_NsStringCatABegin:

  inc rsi
  mov al,byte ptr [rcx+rsi]
  cmp rsi,rdx
  jge _NsStringCatAEnd
  test al,al
  jnz _NsStringCatABegin

_NsStringCatALink:

  inc rdi
  mov al,byte ptr[r8+rdi]
  test al,al
  jz _NsStringCatAEnd
  mov byte ptr [rcx+rsi],al
  inc rsi
  cmp rsi,rdx
  jge _NsStringCatAEnd
  jmp _NsStringCatALink

_NsStringCatAEnd:
  xor rax,rax
  mov byte ptr [rcx+rsi],al
  pop rdi
  pop rsi
  ret

NsStringCatA endp

NsInitDll proc

  push rsi
  sub rsp,130h
  xor rax,rax
  mov [rsp+120],rax

  lea rsi,[rsp+20h]
  mov rdx,100h
  mov rcx,rsi
  call GetSystemDirectoryA
  test rax,rax
  jz _NsInitDllEnd

  lea r8,szDllName
  mov rdx,100h
  mov rcx,rsi
  call NsStringCatA

  mov rcx,rsi
  call LoadLibraryA
  mov [rsi+100h],rax

  test rax,rax
  jz _NsInitDllEnd

  lea rdx,szApplyCompatResolutionQuirking
  mov rcx,[rsi+100h]
  call GetProcAddress
  test rax,rax
  jz _NsInitDllEnd
  mov pfApplyCompatResolutionQuirking, rax

  lea rdx,szCompatString
  mov rcx,[rsi+100h]
  call GetProcAddress
  test rax,rax
  jz _NsInitDllEnd
  mov pfCompatString, rax

  lea rdx,szCompatValue
  mov rcx,[rsi+100h]
  call GetProcAddress
  test rax,rax
  jz _NsInitDllEnd
  mov pfCompatValue, rax

  lea rdx,szCreateDXGIFactory
  mov rcx,[rsi+100h]
  call GetProcAddress
  test rax,rax
  jz _NsInitDllEnd
  mov pfCreateDXGIFactory, rax

  lea rdx,szCreateDXGIFactory1
  mov rcx,[rsi+100h]
  call GetProcAddress
  test rax,rax
  jz _NsInitDllEnd
  mov pfCreateDXGIFactory1, rax

  lea rdx,szCreateDXGIFactory2
  mov rcx,[rsi+100h]
  call GetProcAddress
  test rax,rax
  jz _NsInitDllEnd
  mov pfCreateDXGIFactory2, rax

  lea rdx,szDXGID3D10CreateDevice
  mov rcx,[rsi+100h]
  call GetProcAddress
  test rax,rax
  jz _NsInitDllEnd
  mov pfDXGID3D10CreateDevice, rax

  lea rdx,szDXGID3D10CreateLayeredDevice
  mov rcx,[rsi+100h]
  call GetProcAddress
  test rax,rax
  jz _NsInitDllEnd
  mov pfDXGID3D10CreateLayeredDevice, rax

  lea rdx,szDXGID3D10GetLayeredDeviceSize
  mov rcx,[rsi+100h]
  call GetProcAddress
  test rax,rax
  jz _NsInitDllEnd
  mov pfDXGID3D10GetLayeredDeviceSize, rax

  lea rdx,szDXGID3D10RegisterLayers
  mov rcx,[rsi+100h]
  call GetProcAddress
  test rax,rax
  jz _NsInitDllEnd
  mov pfDXGID3D10RegisterLayers, rax

  lea rdx,szDXGIDeclareAdapterRemovalSupport
  mov rcx,[rsi+100h]
  call GetProcAddress
  test rax,rax
  jz _NsInitDllEnd
  mov pfDXGIDeclareAdapterRemovalSupport, rax

  lea rdx,szDXGIDumpJournal
  mov rcx,[rsi+100h]
  call GetProcAddress
  test rax,rax
  jz _NsInitDllEnd
  mov pfDXGIDumpJournal, rax

  lea rdx,szDXGIGetDebugInterface1
  mov rcx,[rsi+100h]
  call GetProcAddress
  test rax,rax
  jz _NsInitDllEnd
  mov pfDXGIGetDebugInterface1, rax

  lea rdx,szDXGIReportAdapterConfiguration
  mov rcx,[rsi+100h]
  call GetProcAddress
  test rax,rax
  jz _NsInitDllEnd
  mov pfDXGIReportAdapterConfiguration, rax

  lea rdx,szPIXBeginCapture
  mov rcx,[rsi+100h]
  call GetProcAddress
  test rax,rax
  jz _NsInitDllEnd
  mov pfPIXBeginCapture, rax

  lea rdx,szPIXEndCapture
  mov rcx,[rsi+100h]
  call GetProcAddress
  test rax,rax
  jz _NsInitDllEnd
  mov pfPIXEndCapture, rax

  lea rdx,szPIXGetCaptureState
  mov rcx,[rsi+100h]
  call GetProcAddress
  test rax,rax
  jz _NsInitDllEnd
  mov pfPIXGetCaptureState, rax

  lea rdx,szSetAppCompatStringPointer
  mov rcx,[rsi+100h]
  call GetProcAddress
  test rax,rax
  jz _NsInitDllEnd
  mov pfSetAppCompatStringPointer, rax

  lea rdx,szUpdateHMDEmulationStatus
  mov rcx,[rsi+100h]
  call GetProcAddress
  test rax,rax
  jz _NsInitDllEnd
  mov pfUpdateHMDEmulationStatus, rax

  xor rax,rax
  inc rax
  mov [rsp+120],rax

_NsInitDllEnd:

  mov rax,[rsp+120]
  add rsp,130h
  pop rsi
  ret
NsInitDll endp

Hijack64_ApplyCompatResolutionQuirking proc
  jmp qword ptr [pfApplyCompatResolutionQuirking]
Hijack64_ApplyCompatResolutionQuirking endp

Hijack64_CompatString proc
  jmp qword ptr [pfCompatString]
Hijack64_CompatString endp

Hijack64_CompatValue proc
  jmp qword ptr [pfCompatValue]
Hijack64_CompatValue endp

Hijack64_CreateDXGIFactory proc
  jmp qword ptr [pfCreateDXGIFactory]
Hijack64_CreateDXGIFactory endp

Hijack64_CreateDXGIFactory1 proc
  jmp qword ptr [pfCreateDXGIFactory1]
Hijack64_CreateDXGIFactory1 endp

Hijack64_CreateDXGIFactory2 proc
  jmp qword ptr [pfCreateDXGIFactory2]
Hijack64_CreateDXGIFactory2 endp

Hijack64_DXGID3D10CreateDevice proc
  jmp qword ptr [pfDXGID3D10CreateDevice]
Hijack64_DXGID3D10CreateDevice endp

Hijack64_DXGID3D10CreateLayeredDevice proc
  jmp qword ptr [pfDXGID3D10CreateLayeredDevice]
Hijack64_DXGID3D10CreateLayeredDevice endp

Hijack64_DXGID3D10GetLayeredDeviceSize proc
  jmp qword ptr [pfDXGID3D10GetLayeredDeviceSize]
Hijack64_DXGID3D10GetLayeredDeviceSize endp

Hijack64_DXGID3D10RegisterLayers proc
  jmp qword ptr [pfDXGID3D10RegisterLayers]
Hijack64_DXGID3D10RegisterLayers endp

Hijack64_DXGIDeclareAdapterRemovalSupport proc
  jmp qword ptr [pfDXGIDeclareAdapterRemovalSupport]
Hijack64_DXGIDeclareAdapterRemovalSupport endp

Hijack64_DXGIDumpJournal proc
  jmp qword ptr [pfDXGIDumpJournal]
Hijack64_DXGIDumpJournal endp

Hijack64_DXGIGetDebugInterface1 proc
  jmp qword ptr [pfDXGIGetDebugInterface1]
Hijack64_DXGIGetDebugInterface1 endp

Hijack64_DXGIReportAdapterConfiguration proc
  jmp qword ptr [pfDXGIReportAdapterConfiguration]
Hijack64_DXGIReportAdapterConfiguration endp

Hijack64_PIXBeginCapture proc
  jmp qword ptr [pfPIXBeginCapture]
Hijack64_PIXBeginCapture endp

Hijack64_PIXEndCapture proc
  jmp qword ptr [pfPIXEndCapture]
Hijack64_PIXEndCapture endp

Hijack64_PIXGetCaptureState proc
  jmp qword ptr [pfPIXGetCaptureState]
Hijack64_PIXGetCaptureState endp

Hijack64_SetAppCompatStringPointer proc
  jmp qword ptr [pfSetAppCompatStringPointer]
Hijack64_SetAppCompatStringPointer endp

Hijack64_UpdateHMDEmulationStatus proc
  jmp qword ptr [pfUpdateHMDEmulationStatus]
Hijack64_UpdateHMDEmulationStatus endp
ENDIF

end
