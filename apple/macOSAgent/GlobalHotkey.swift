import Carbon.HIToolbox

/// Registers a system-wide hotkey (default Ctrl+Shift+L) via the Carbon
/// RegisterEventHotKey API and invokes a callback when it fires.
final class GlobalHotkey {
    private var hotKeyRef: EventHotKeyRef?
    private var eventHandler: EventHandlerRef?
    private var callback: (() -> Void)?

    /// keyCode 0x25 == ANSI 'L'; modifiers control+shift.
    func register(keyCode: UInt32 = UInt32(kVK_ANSI_L),
                  modifiers: UInt32 = UInt32(controlKey | shiftKey),
                  _ callback: @escaping () -> Void) {
        self.callback = callback

        var eventType = EventTypeSpec(eventClass: OSType(kEventClassKeyboard),
                                      eventKind: UInt32(kEventHotKeyPressed))
        let selfPtr = Unmanaged.passUnretained(self).toOpaque()

        InstallEventHandler(GetApplicationEventTarget(), { _, _, userData in
            guard let userData else { return noErr }
            let me = Unmanaged<GlobalHotkey>.fromOpaque(userData).takeUnretainedValue()
            me.callback?()
            return noErr
        }, 1, &eventType, selfPtr, &eventHandler)

        let hotKeyID = EventHotKeyID(signature: OSType(0x564F4342 /* 'VOCB' */), id: 1)
        RegisterEventHotKey(keyCode, modifiers, hotKeyID, GetApplicationEventTarget(), 0, &hotKeyRef)
    }

    deinit {
        if let hotKeyRef { UnregisterEventHotKey(hotKeyRef) }
        if let eventHandler { RemoveEventHandler(eventHandler) }
    }
}
