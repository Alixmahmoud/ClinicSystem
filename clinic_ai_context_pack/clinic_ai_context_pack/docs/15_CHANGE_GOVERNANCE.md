# Implementation and Change Governance

## Governing Rules
1. A later implementation problem does not automatically reopen an earlier frozen phase.
2. Every implementation change must preserve:
   - aggregate boundaries;
   - business-operation semantics;
   - synchronization semantics;
   - authorization semantics;
   - clinical-history semantics;
   - financial-history semantics;
   - appointment/queue semantics;
   - Follow-Up / Task / Exception distinctions;
   - correction/addendum semantics;
   - backup/restore boundaries;
   - migration semantics.
3. When implementation convenience conflicts with a frozen business rule, implementation must adapt unless an explicit phase-gate process authorizes a change.
4. Open questions remain open until resolved through the appropriate project phase.
5. Proposed and implementation-specific choices must never be silently promoted into frozen architecture/data/product decisions.
6. The First Vertical Slice validates the frozen system; it does not grant permission to redesign it.
7. Integration/failure testing must preserve semantic distinctions rather than collapsing everything into generic errors.
8. Pilot/release hardening must remain inside confirmed V1 scope and is not an unapproved product expansion.

## Defect Classification
Before changing frozen content, classify the problem as:
- implementation defect
- specification gap
- contradiction
- true upstream decision issue

## Source
Master handoff pages 16–18.
