import Foundation

/// Companion-app message only. Send through WatchConnectivity; never include a Gateway credential.
public struct DMWatchOperation: Codable, Sendable {
    public let operationId: String
    public let title: String
    public let status: String
    public let message: String
    public let sequence: Int64
    public let isFinished: Bool
    public let canCancel: Bool

    public init(operation: DMOperation, title: String, grants: [String]) {
        operationId = operation.id
        self.title = title
        status = operation.status
        message = operation.message
        sequence = operation.sequence
        isFinished = operation.isFinished
        canCancel = grants.contains("operations-cancel") && operation.canCancel && !operation.isFinished && !operation.isStale
    }
}

public struct DMWatchOperationState: Sendable {
    public private(set) var latest: DMWatchOperation?
    public init() {}
    public mutating func accept(_ update: DMWatchOperation) -> Bool {
        if let latest, latest.operationId == update.operationId && update.sequence <= latest.sequence { return false }
        latest = update
        return true
    }
}
