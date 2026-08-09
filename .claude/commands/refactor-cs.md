---
description: Refactor C# files to the csharp-unity skill standard with a multi-agent workflow (refactor + independent verify)
argument-hint: [path or folder] [--dry-run] [--batch N]
allowed-tools: Workflow, Bash(git:*), Read, Glob, Grep
---

Run the `refactor-csharp-unity` workflow to apply the **csharp-unity** skill across this project's own scripts.

This is an explicit request for multi-agent orchestration — call the `Workflow`
tool with `{ name: "refactor-csharp-unity", args: {...} }`.

## Building `args` from `$ARGUMENTS`

| Input | `args` |
|---|---|
| empty | `{}` — defaults to `Assets/_Assets/Script` |
| a path or folder | `{ scope: "<path>" }` |
| several paths | `{ scope: "<path a>, <path b>" }` |
| contains `--dry-run` | add `dryRun: true` — reports what it would change, writes nothing |
| contains `--batch N` | add `filesPerBatch: N` (default 6; lower = more agents, smaller context each) |

If the user said "the files I changed" or similar, resolve the paths first with
`git status --short` / `git diff --name-only` and pass them as `scope`.

**Never widen the scope to third-party code** — `Assets/Plugins`, `Assets/Sirenix`,
`Assets/FindReference2`, `Assets/UIExtensions`, `Assets/ThirdParty`,
`Assets/TutorialInfo`, `Assets/TextMesh Pro`. The workflow excludes them, and so
should any scope you pass. If the user explicitly asks for one of those, push back
once: restyling vendored code makes every future update a merge conflict.

## Before you launch it

State the scope and the agent count (roughly `2 × ceil(files / batch size) + 1`)
and confirm, unless the user already named a narrow scope or asked for a dry run.
The workflow edits files in place, so the working tree should be clean or
committed first — check with `git status --short` and say so if it is not.

## After it returns

The workflow returns per-batch results. Report:

- files changed, and the notable structural changes
- every `risks` entry from the refactor agents and every unfixed `violations`
  entry from the verifiers — these are the items that need a human decision
- any file whose `styleOnlyDiff` is `false`, loudly: that means behavior may
  have shifted and the diff needs review

Then remind the user to let Unity recompile and to open the affected scenes: this
is a game project, so scenes and prefabs reference serialized field names and
UnityEvent method names as strings, and a missing reference shows up in the
Inspector rather than in the compiler.
