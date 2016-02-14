module VirtualRadar.Interface.Network {
    export const enum ConnectionStatus {
        Disconnected = 0,
        Connecting = 1,
        Connected = 2,
        CannotConnect = 3,
        Reconnecting = 4,
        Waiting = 5
    }
}
module VirtualRadar.Interface.Settings {
    export const enum ReceiverUsage {
        Normal = 0,
        HideFromWebSite = 1,
        MergeOnly = 2
    }
}

