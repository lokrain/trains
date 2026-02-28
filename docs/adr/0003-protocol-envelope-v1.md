# ADR 0003: Protocol Envelope v1

## Status
Accepted

## Decision
Adopt a strict little-endian binary envelope with versioned header.
Envelope includes lane, message type, sequence, message id, and payload length.

## Consequences
- Forward-compatible protocol evolution via version checks.
- Fast bounds validation and malformed-payload rejection.
- Clean lane separation (RO/US) in transport integration.
