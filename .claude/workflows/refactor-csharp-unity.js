export const meta = {
  name: 'refactor-csharp-unity',
  description: 'Refactor C# / Unity files to the csharp-unity skill standard, then adversarially verify each file',
  whenToUse:
    'When applying the csharp-unity skill to existing code — region layout, naming, no comments, no hard-code, this. prefix, var, early return, LINQ removal. Pass a folder or a file list; defaults to the project\'s own scripts under Assets/_Assets/Script.',
  phases: [
    { title: 'Discover', detail: 'resolve the target scope into a concrete .cs file list' },
    { title: 'Refactor', detail: 'one agent per batch, rewrites files in place' },
    { title: 'Verify', detail: 'independent agent re-reads each batch and fixes leftovers' },
  ],
}

const SKILL_PATH = 'C:/Users/Kingfisher/.claude/skills/csharp-unity/SKILL.md'
const RUNTIME_REFERENCE = 'C:/Users/Kingfisher/.claude/skills/csharp-unity/references/runtime.md'
const DEFAULT_SCOPE = 'Assets/_Assets/Script'
const DEFAULT_BATCH = 6

const THIRD_PARTY = [
  'Assets/Plugins',
  'Assets/Sirenix',
  'Assets/FindReference2',
  'Assets/UIExtensions',
  'Assets/ThirdParty',
  'Assets/TutorialInfo',
  'Assets/TextMesh Pro',
]

const FILE_LIST_SCHEMA = {
  type: 'object',
  properties: {
    files: { type: 'array', items: { type: 'string' } },
    skipped: { type: 'array', items: { type: 'string' } },
  },
  required: ['files'],
}

const REFACTOR_SCHEMA = {
  type: 'object',
  properties: {
    files: {
      type: 'array',
      items: {
        type: 'object',
        properties: {
          path: { type: 'string' },
          changed: { type: 'boolean' },
          changes: { type: 'array', items: { type: 'string' } },
          risks: { type: 'array', items: { type: 'string' } },
        },
        required: ['path', 'changed'],
      },
    },
  },
  required: ['files'],
}

const VERIFY_SCHEMA = {
  type: 'object',
  properties: {
    files: {
      type: 'array',
      items: {
        type: 'object',
        properties: {
          path: { type: 'string' },
          styleOnlyDiff: { type: 'boolean' },
          violations: {
            type: 'array',
            items: {
              type: 'object',
              properties: {
                rule: { type: 'string' },
                detail: { type: 'string' },
                fixed: { type: 'boolean' },
              },
              required: ['rule', 'detail', 'fixed'],
            },
          },
          revertedHunks: { type: 'array', items: { type: 'string' } },
        },
        required: ['path', 'styleOnlyDiff'],
      },
    },
  },
  required: ['files'],
}

const SKILL_HEADER = `You are refactoring C# / Unity code in the lily-of-valley repository — a Unity GAME project (Windows, repo root D:/KingfisherWork/lily-of-valley). The code you touch is runtime gameplay and its editor tooling, wired to scenes and prefabs.

AUTHORITY: read ${SKILL_PATH} in full before touching a file. It is the single source of truth for style. Read it even if you think you remember it — it was rewritten recently. For runtime gameplay code also read ${RUNTIME_REFERENCE}.

Its non-negotiables, summarized (the file wins on any conflict):
- Regions, in this exact order: "Field" (private enums declared in the class first, then consts, static fields, serialized fields, private fields, events), "Property" (every property), "Unity Lifecycle" (every Unity message in ONE region, call order), purpose regions (ONLY when the class has more than 3 non-lifecycle methods; each needs at least 2 methods; constructors do not count), "Method" (leftover methods), "Nested Type" (nested classes/structs and non-private nested enums, always last). A type inside "Nested Type" declares no regions of its own.
- Never nest regions. One blank line before and after every #region and #endregion line, except directly against a class brace. No empty regions.
- Blank line after every flow-exit statement (return, break, continue, throw, goto, yield break) that is not the last line of its block, and after the closing brace of a multi-line guard. In a switch, that means a blank line between cases. One blank line between members; never two in a row; never right after { or before }.
- Naming: Unity conventions primary, Rider fills the gaps. Private fields _camelCase; [SerializeField] private fields camelCase with NO underscore; const and static readonly PascalCase; methods start with a verb; bools prefixed is/has/can/should; interfaces I-prefixed; enums singular.
- A serialized field is always "[SerializeField] private" and stays private. If something outside the class reads it, add a property in the Property region: "public float MoveSpeed => this.moveSpeed;". Never [field: SerializeField], never a public serialized field. Add the property only where an existing caller needs it — this is a style refactor, not an API expansion.
- this. on every instance field access, and on NOTHING else. Never on a property, never on an event (a field-like event is an event, not a field — invoke it bare: "HealthChanged?.Invoke(x);"), never on a method, local, param or static. Strip every other redundant qualifier too: no "base." unless the member is shadowed, no namespace qualifier the file's usings already cover (keep UnityEngine.Object where "using System;" makes bare Object ambiguous).
- var for locals whenever the right-hand side makes the type obvious.
- Early return instead of nested ifs.
- NO comments at all. Delete every existing comment, banner, divider and commented-out block. Only /// <summary> on cross-assembly public API survives.
- No hard-code: magic numbers, strings, layer/tag names, animator parameters and input action names become const / static readonly / [SerializeField] fields with descriptive names. Cache Animator.StringToHash and Shader.PropertyToID.
- No LINQ — plain for/foreach loops. Never in Update, FixedUpdate, LateUpdate, OnGUI, or anything called per frame.
- Runtime hygiene: cache GetComponent results in Awake, never per frame; explicit == null checks on UnityEngine.Object (no ?.); cache WaitForSeconds; subscribe in OnEnable and unsubscribe in OnDisable; delete empty Update methods.
- Simple, readable, scalable: short methods, one job each, no speculative abstraction.
- Finish with the skill's own "Self-Check" list at the bottom of SKILL.md — run every item over each file you touched before you report.`

const SAFETY_RULES = `HARD SAFETY RULES — a violation is worse than an unrefactored file:
1. BEHAVIOR MUST NOT CHANGE. This is a pure style/structure refactor. No logic edits, no "while I'm here" bug fixes, no API redesign. If you spot a real bug, report it in "risks" and leave the code alone.
2. THE SCENES AND PREFABS ARE CONSUMERS OF THIS CODE, and they reference it BY NAME in text you are not editing:
   - Serialized field names — renaming one silently discards the value saved in every scene, prefab and asset. This includes public fields and [SerializeField] private fields on MonoBehaviours, ScriptableObjects and [Serializable] classes. It outranks the naming table; the skill says so under "Serialized names are frozen".
   - Method names bound to UnityEvents (Button.onClick, EventTrigger, PlayableDirector signals, Animation Events) are stored as strings in the scene. Renaming a public method can silently break a button.
   - Anything reached by string: SendMessage, Invoke("Name"), StartCoroutine("Name"), Animator parameter and state names, Input System action names, scene names, Resources paths, layer and tag names.
   Do not rename ANY of these. Extracting a repeated literal to a const is fine — changing its VALUE is not.
3. DO NOT RENAME public or internal types, methods, properties, events, or enum members, and do not rename files. Renames are allowed ONLY for private non-serialized fields, locals, and parameters, and only when the file is not "partial".
4. NEVER edit third-party code: ${THIRD_PARTY.join(', ')}, or anything under Library/, Temp/, obj/, Packages/. If a batch path points there, skip it and say so.
5. Odin (Sirenix) attributes — [Button], [ShowInInspector], [FoldoutGroup], [TitleGroup] and friends — are load-bearing Inspector wiring. Keep every attribute exactly as it is, on the member it is attached to.
6. Preserve #if UNITY_EDITOR guards, using directive order, namespace, and file-scoped vs block-scoped namespace style.
7. Keep the file compiling: balanced braces, every region closed, no orphaned members.`

const options = normalizeArgs(args)

phase('Discover')

const discovery = await agent(
  `List every C# file to refactor in D:/KingfisherWork/lily-of-valley.

Scope requested: ${options.scope}

Rules:
- Include only .cs files that exist under that scope.
- EXCLUDE all third-party code: ${THIRD_PARTY.join(', ')}.
- EXCLUDE Library/, Temp/, obj/, Logs/, Packages/, and any *.g.cs / AssemblyInfo.cs / *.designer.cs.
- Return repo-relative paths with forward slashes.
- Put excluded-but-matching paths in "skipped" with a one-word reason.
Return the list only — do not read or edit the files.`,
  { label: 'discover', phase: 'Discover', schema: FILE_LIST_SCHEMA },
)

const files = discovery && discovery.files ? discovery.files : []

if (files.length === 0) {
  log(`No C# files found under ${options.scope} — nothing to refactor.`)
  return { scope: options.scope, fileCount: 0, batches: [] }
}

const batches = buildBatches(files, options.filesPerBatch)

log(
  `${files.length} file(s) in ${batches.length} batch(es) of up to ${options.filesPerBatch}` +
    (options.dryRun ? ' — DRY RUN, nothing will be written.' : '') +
    (discovery.skipped && discovery.skipped.length
      ? ` Skipped ${discovery.skipped.length} third-party/generated path(s).`
      : ''),
)

const results = await pipeline(
  batches,
  (batch) =>
    agent(refactorPrompt(batch), {
      label: `refactor:${batch.label}`,
      phase: 'Refactor',
      schema: REFACTOR_SCHEMA,
    }),
  (refactored, batch) =>
    agent(verifyPrompt(batch, refactored), {
      label: `verify:${batch.label}`,
      phase: 'Verify',
      schema: VERIFY_SCHEMA,
    }).then((verdict) => ({ batch: batch.label, files: batch.files, refactored, verdict })),
)

const completed = results.filter(Boolean)

return {
  scope: options.scope,
  dryRun: options.dryRun,
  fileCount: files.length,
  batchCount: batches.length,
  failedBatches: batches.length - completed.length,
  skipped: discovery.skipped || [],
  batches: completed,
  reminder: options.dryRun
    ? 'Dry run — nothing was written.'
    : 'Let Unity recompile, then open the affected scenes and confirm Inspector references and UnityEvent bindings are still wired before committing.',
}

function refactorPrompt(batch) {
  const action = options.dryRun
    ? 'DRY RUN — do NOT edit any file. For each file, report exactly what you WOULD change in "changes".'
    : 'Rewrite each file in place with the Edit tool (or Write for a full restructure). Apply every rule — a partially refactored file is a failure.'

  return `${SKILL_HEADER}

${SAFETY_RULES}

TASK — refactor these ${batch.files.length} file(s):
${batch.files.map((path) => `- ${path}`).join('\n')}

${action}

Method:
1. Read ${SKILL_PATH}.
2. Read every file in the batch first — they live in the same folder and reference each other. Know the cross-file surface before you edit.
3. Before renaming any private field, grep the repo for its name as a string — Odin, UnityEvents and SerializedObject code can reach private fields by name too.
4. Per file: fix region layout first (split Field / Property, private enums to the top of Field, nested types into a trailing "Nested Type" region), then naming (within the rename limits), then this./var/early-return, then strip every comment, then extract hard-coded literals to named consts.
5. Re-read your own output and confirm the braces balance and every region is closed.

For each file report: path, whether you changed it, a short bullet per class of change you applied, and "risks" for anything you deliberately left alone (suspected bug, a rename that would break scene data or a UnityEvent binding, ambiguity worth a human decision).`
}

function verifyPrompt(batch, refactored) {
  const claimed = refactored && refactored.files ? JSON.stringify(refactored.files) : '[]'

  return `${SKILL_HEADER}

You are the INDEPENDENT VERIFIER. Another agent just refactored these files. Assume it missed things — your job is to find what it missed, not to agree with it.

Files:
${batch.files.map((path) => `- ${path}`).join('\n')}

What it claims it did:
${claimed}

${SAFETY_RULES}

Method:
1. Read ${SKILL_PATH}, then read each file as it exists on disk NOW. Do not trust the claims above.
2. Check every rule, file by file:
   - "Field" is first and holds ALL fields, consts and events, with private enums at its very top; "Property" comes second and holds every property
   - exactly one "Unity Lifecycle" region, in call order, with no lifecycle method living elsewhere
   - purpose regions only above 3 non-lifecycle methods, each with 2+ methods, leftovers in "Method"
   - "Nested Type" is the last region and contains every nested class/struct and non-private nested enum, none of which declare regions of their own
   - no nested regions, no empty regions, blank line around every directive
   - blank line after every return / break / continue / throw that is not the last line of its block, and between switch cases
   - naming table compliance, including _camelCase private fields and no-underscore serialized fields
   - every serialized field is [SerializeField] private, exposed only through a property in "Property"
   - this. on every instance field access, and on nothing else — flag any this. on a property, event or method, and any other redundant qualifier
   - var on obvious locals
   - ZERO comments remain
   - no magic numbers or repeated string literals left inline
   - no LINQ introduced or left behind, and nothing allocating per frame
3. Run \`git diff -- <file>\` for each file and confirm the diff is style-only. Any behavioral difference, renamed serialized field, renamed public method, or changed string literal value is a DEFECT — revert that specific hunk to the original.
${options.dryRun ? '4. DRY RUN: report violations, do NOT edit anything.' : '4. Fix every violation you find, directly in the file.'}

Report per file: remaining violations (rule + detail, and whether you fixed it), whether the diff is style-only, and any hunk you reverted. Be precise; do not pad the list with things that are already correct.`
}

function normalizeArgs(input) {
  const defaults = { scope: DEFAULT_SCOPE, filesPerBatch: DEFAULT_BATCH, dryRun: false }
  if (!input) return defaults
  if (typeof input === 'string') return { ...defaults, scope: input }
  if (Array.isArray(input)) return { ...defaults, scope: input.join(', ') }

  return {
    scope: input.scope || (Array.isArray(input.files) ? input.files.join(', ') : DEFAULT_SCOPE),
    filesPerBatch: input.filesPerBatch > 0 ? input.filesPerBatch : DEFAULT_BATCH,
    dryRun: input.dryRun === true,
  }
}

function buildBatches(paths, size) {
  const byFolder = new Map()
  for (const path of paths) {
    const cut = path.lastIndexOf('/')
    const folder = cut < 0 ? '.' : path.slice(0, cut)
    if (!byFolder.has(folder)) byFolder.set(folder, [])
    byFolder.get(folder).push(path)
  }

  const batches = []
  for (const [folder, folderFiles] of byFolder) {
    const name = folder.slice(folder.lastIndexOf('/') + 1) || folder
    const chunkCount = Math.ceil(folderFiles.length / size)
    const chunkSize = Math.ceil(folderFiles.length / chunkCount)
    for (let i = 0; i < folderFiles.length; i += chunkSize) {
      const label = chunkCount > 1 ? `${name}#${Math.floor(i / chunkSize) + 1}` : name
      batches.push({ label, folder, files: folderFiles.slice(i, i + chunkSize) })
    }
  }

  return batches
}
