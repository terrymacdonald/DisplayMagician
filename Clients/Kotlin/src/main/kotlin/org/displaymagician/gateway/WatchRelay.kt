package org.displaymagician.gateway

import org.json.JSONObject

/** Companion-app message for Wear OS Data Layer. Gateway credentials stay on the phone. */
data class DMWatchOperation(val operationId: String, val title: String, val status: String, val message: String, val sequence: Long, val isFinished: Boolean, val canCancel: Boolean) {
    companion object {
        fun fromOperation(operation: DMOperation, title: String, grants: List<String>) = DMWatchOperation(operation.id, title, operation.status, operation.message, operation.sequence, operation.isFinished,
            "operations-cancel" in grants && operation.canCancel && !operation.isFinished && !operation.isStale)
    }
    fun toJson() = JSONObject().put("operationId", operationId).put("title", title).put("status", status).put("message", message).put("sequence", sequence).put("isFinished", isFinished).put("canCancel", canCancel)
}

class DMWatchOperationState {
    var latest: DMWatchOperation? = null
        private set
    fun accept(update: DMWatchOperation): Boolean {
        val previous = latest
        if (previous != null && previous.operationId == update.operationId && update.sequence <= previous.sequence) return false
        latest = update
        return true
    }
}
