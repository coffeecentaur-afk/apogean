# Research: Project-wide Wayfinder star chart

**Target:** [coffeecentaur-afk/apogean issue #14, “Prototype a project-wide Wayfinder star chart”](https://github.com/coffeecentaur-afk/apogean/issues/14)
**Research date:** 2026-09-01
**Scope:** Read-only visualization research. This report does not implement a UI, change project code or assets, mutate issues, commit, or push.

## Executive conclusion

A read-only project-wide Wayfinder can use GitHub Issues as its canonical work and decision data. GitHub now exposes issue state and state reason, labels, assignees, parent/sub-issue hierarchy, and both directions of issue dependencies through supported APIs. The current Apogean repository already uses these features: [issue #1](https://github.com/coffeecentaur-afk/apogean/issues/1) is a `wayfinder:map` parent with twelve sub-issues, two completed decisions, and dependency edges that distinguish immediately discussable decisions from blocked ones.

GitHub should remain canonical for **meaning and status**, but it should not be forced to store presentation concerns such as star coordinates, nebula contours, or camera position. For the MVP, positions can be generated deterministically from issue hierarchy and dependency depth. Fog should be a derived visual region, not a fake task. If hand-authored positions are eventually necessary, a small versioned view-layout file may supplement GitHub without duplicating issue state.

The recommended MVP is:

1. A build-time, read-only GraphQL adapter that normalizes GitHub issues into a small JSON graph.
2. A custom SVG renderer using selected D3 modules (`d3-hierarchy`, `d3-shape`, `d3-zoom`, and optionally `d3-force` only during layout).
3. A repository-native Mermaid snapshot as a low-cost fallback in Markdown, generated from the same normalized data.
4. A static deployment, preferably GitHub Pages, with no GitHub credential shipped to the browser.

D3/SVG is the best fit at Apogean’s expected scale because it permits an authored star-chart appearance, accessible DOM content, CSS styling, masks, filters, deterministic screenshots, and direct links to issues. Cytoscape.js is the strongest fallback if graph analysis and automatic compound layouts become more important than bespoke art. Sigma/Graphology and PixiJS are technically capable but premature; their WebGL strengths matter at thousands of nodes, while they increase custom accessibility, label, layout, and security work.

## 1. Can GitHub Issues be canonical?

### Supported source data

The standard issue representation includes stable IDs/numbers, title, body, state, state reason, labels, assignees, milestone, timestamps, and the canonical HTML URL. GitHub’s Issues API explicitly supports viewing and managing issue metadata, labels, and assignees ([REST Issues](https://docs.github.com/en/rest/issues/issues), [labels](https://docs.github.com/en/rest/issues/labels), [assignees](https://docs.github.com/en/rest/issues/assignees)).

Hierarchy and prerequisites are first-class relationships rather than conventions hidden in Markdown:

- The REST API provides parent and sub-issue endpoints, including paginated listing of sub-issues ([sub-issues endpoints](https://docs.github.com/en/rest/issues/sub-issues)). GitHub supports up to 100 direct sub-issues and eight hierarchy levels per parent ([sub-issue product documentation](https://docs.github.com/en/issues/tracking-your-work-with-issues/using-issues/adding-sub-issues)).
- The REST API lists both dependencies an issue is blocked by and issues it blocks ([issue dependency endpoints](https://docs.github.com/en/rest/issues/issue-dependencies)).
- The current GraphQL `Issue` schema exposes `parent`, `subIssues`, `subIssuesSummary`, `blockedBy`, `blocking`, `issueDependenciesSummary`, `labels`, `assignees`, `state`, and `stateReason` ([GraphQL Issues reference](https://docs.github.com/en/graphql/reference/issues)).

GraphQL is the preferred adapter for the chart because it can request node metadata and relationships together and avoid a REST request per issue. Every connection must still be cursor-paginated with `first` or `last` values from 1–100, and a query cannot request more than 500,000 nodes ([GraphQL pagination](https://docs.github.com/en/graphql/guides/using-pagination-in-the-graphql-api), [GraphQL limits](https://docs.github.com/en/graphql/overview/rate-limits-and-query-limits-for-the-graphql-api)). REST remains useful for small scripts, debugging, and an independently testable fallback.

### Evidence from the current repository

The read-only snapshot on 2026-09-01 is already chartable:

- `#1 Chart the Act 1 environmental foundation` is labeled `wayfinder:map` and owns twelve sub-issues.
- `#2` and `#3` are closed with `COMPLETED` state reason, so they are resolved destinations rather than merely hidden tasks.
- `#4`, `#5`, `#6`, `#7`, and `#8` are open with no open prerequisites and are therefore the current frontier.
- `#9` and `#10` are blocked by `#4`; `#11` is blocked by `#7`; `#12` is blocked by `#5`, `#8`, `#9`, and `#10`; and `#13` is the convergence gate blocked by the remaining environmental decisions.
- `#14`, `#15`, and `#16` are outside the `#1` hierarchy and can appear as distant named regions rather than being mixed into the current environmental map.

This proves that status and topology do not need a parallel database. It also exposes one important rule: **open does not mean frontier**. Frontier must be derived as “open and all `blockedBy` issues resolved,” not from state or label alone.

### Recommended normalized graph contract

The adapter should emit only fields the renderer needs:

| Entity | Required normalized fields |
|---|---|
| Issue node | repository, node ID, number, title, URL, state, state reason, labels, assignees, parent ID, updated timestamp |
| Hierarchy edge | parent issue ID, child issue ID, sub-issue priority/order |
| Dependency edge | blocking issue ID, blocked issue ID |
| Map summary | root issue ID, completed/total count, generated timestamp, source revision/query version |

The renderer should never interpret arbitrary issue-body HTML. Titles, labels, and assignee names must be inserted as text, not `innerHTML`. Markdown bodies can remain on GitHub and open in a new tab; excluding them from the MVP sharply reduces injection risk and payload size.

### What GitHub does not provide

GitHub does not define celestial coordinates, camera framing, region silhouettes, visual fog, or decorative routes. These are presentation data. The MVP should derive them deterministically:

- top-level `wayfinder:map` issues define sectors;
- sub-issue order defines angular order within a sector;
- dependency depth defines orbital radius or horizontal progression;
- stable issue number breaks layout ties;
- closed maps collapse to small labeled constellations;
- disconnected future issues sit in a distant “later” belt.

Only introduce a checked-in layout file if deterministic layout repeatedly fails visual review. If added, it should contain IDs and optional coordinates/groups only—never duplicated title, state, labels, or dependency status.

## 2. Rendering candidates

| Candidate | Strengths for Wayfinder | Costs/limits | License and maintenance | Verdict |
|---|---|---|---|---|
| GitHub Mermaid | Renders directly in Markdown, issues, pull requests, and wikis; no custom hosting; excellent fallback and architecture snapshot ([GitHub diagram support](https://docs.github.com/en/get-started/writing-on-github/working-with-advanced-formatting/creating-diagrams)) | Limited bespoke celestial composition, fog, camera interaction, and dense-label control; output depends on GitHub’s Mermaid version | MIT ([license](https://github.com/mermaid-js/mermaid/blob/develop/LICENSE)); highly active repository ([repo](https://github.com/mermaid-js/mermaid)); recent security fixes make version pinning important for self-hosting ([releases](https://github.com/mermaid-js/mermaid/releases)) | Keep as generated fallback, not primary experience |
| Custom SVG + D3 modules | Maximum visual control; SVG masks/filters support fog and nebulae; DOM nodes permit keyboard focus, text alternatives, links, and deterministic snapshots; D3 supports hierarchy, force, SVG/Canvas rendering, and pan/zoom ([tree](https://d3js.org/d3-hierarchy/tree), [force](https://d3js.org/d3-force), [zoom](https://d3js.org/d3-zoom)) | Requires designing node/edge grammar, collision handling, focus order, and responsive labels | D3 uses ISC ([license](https://github.com/d3/d3/blob/main/LICENSE)); mature and actively maintained even though the umbrella v7.9.0 release is stable rather than frequent ([repo](https://github.com/d3/d3), [releases](https://github.com/d3/d3/releases)) | **Recommended MVP** |
| Cytoscape.js | Purpose-built graph model, selectors, events, data-driven styling, compound nodes, graph analysis, included and extension layouts; strong for dependency exploration ([official docs/demo](https://js.cytoscape.org/)) | Canvas-first rendering is less naturally accessible and less convenient for atmospheric SVG art; rich styling and curved/multiple edges can become expensive at scale ([performance guidance](https://js.cytoscape.org/)) | MIT ([license](https://github.com/cytoscape/cytoscape.js/blob/unstable/LICENSE)); active release cadence and a published security policy ([repo](https://github.com/cytoscape/cytoscape.js), [security](https://github.com/cytoscape/cytoscape.js/security)) | Best fallback if graph behavior outweighs authored art |
| Sigma.js + Graphology | WebGL renderer designed for thousands of nodes/edges; Graphology provides a robust graph model, algorithms, events, and serialization ([Sigma repo](https://github.com/jacomyal/sigma.js), [Graphology repo](https://github.com/graphology/graphology)) | Overkill for tens or low hundreds of issues; labels, depth/opacity, custom node programs, DOM accessibility, and fog overlays require more engineering; Sigma v4 is pre-release territory, so a prototype should stay on stable v3 | Both MIT ([Sigma license](https://github.com/jacomyal/sigma.js/blob/main/LICENSE.txt), [Graphology license](https://github.com/graphology/graphology/blob/master/LICENSE.txt)); both repositories are active; maintainers document continuing renderer changes ([Sigma roadmap](https://github.com/jacomyal/sigma.js/discussions/1469)) | Reserve for a future multi-project graph with thousands of nodes |
| PixiJS/WebGL/WebGPU | Excellent 2D compositing, particles, masks, filters, blend modes, sprites, and very high rendering throughput ([official repository](https://github.com/pixijs/pixijs)) | It is a rendering engine, not a graph library: layout, edge routing, selection semantics, labels, keyboard navigation, and accessible mirrors are custom work | MIT ([license](https://github.com/pixijs/pixijs/blob/dev/LICENSE)); active v8 project ([releases](https://github.com/pixijs/pixijs/releases)) | Use only for a later visual-effects layer if SVG proves insufficient |
| React Flow | Ready-made pan/zoom, controls, minimap, custom DOM nodes, links, and unusually good keyboard/screen-reader support ([viewport](https://reactflow.dev/learn/concepts/the-viewport), [accessibility](https://reactflow.dev/learn/advanced-use/accessibility), [minimap](https://reactflow.dev/api-reference/components/minimap)) | React dependency; no built-in automatic layout, so Dagre, D3, or ELK is still required ([layout overview](https://reactflow.dev/learn/layouting/layouting)); its default visual language reads as an editor/workflow rather than a star chart | Core is MIT and active ([repo](https://github.com/xyflow/xyflow)); some advanced examples are React Flow Pro and carry a separate Pro license ([Pro example notice](https://reactflow.dev/examples/layout/auto-layout)) | Credible rapid prototype, but D3/SVG fits the art direction better |

### Verified visual-reference projects

- [d3-celestial](https://github.com/ofrohn/d3-celestial) demonstrates an interactive Canvas constellation map with zoom/rotation and GeoJSON. Its BSD-3-Clause code is usable under its license, but it is astronomy-specific, its last source push was in 2024, and its bundled sky data comes from multiple external catalogues. Treat it as composition/interaction reference only; do not import its datasets or architecture wholesale ([license](https://github.com/ofrohn/d3-celestial/blob/master/LICENSE)).
- [CommanderFoo/skill-tree-planner](https://github.com/CommanderFoo/skill-tree-planner) is an MIT-licensed example of a state-driven skill-tree editor, but it has a tiny adoption/maintenance signal and is an application rather than a reusable foundation. Its useful lesson is the visual distinction among unlocked, available, and locked nodes—not its codebase.
- [Serdabel/skilltree](https://github.com/Serdabel/skilltree) demonstrates a recent React Flow + Dagre roadmap grammar with completed, in-progress, available, and locked states. GitHub currently detects no repository license, so its code and assets should not be copied.
- [rot.js](https://github.com/ondras/rot.js) provides BSD-3-Clause field-of-view algorithms and an official FOV demo ([manual](https://ondras.github.io/rot.js/manual/)). Its grid visibility algorithms solve line-of-sight, not semantic uncertainty. They are unnecessary for Wayfinder fog; a renderer mask driven by node state is simpler and more truthful.

## 3. MVP visual grammar

The chart should remain legible without its labels, animation, or color. Shape, halo, edge treatment, opacity, and region placement must carry redundant meaning.

| Wayfinder meaning | GitHub-derived rule | Visual treatment | Interaction |
|---|---|---|---|
| Resolved decision | `state=CLOSED` and `stateReason=COMPLETED` | Solid star, stable circular halo, high-contrast core; constellation line remains visible | Opens canonical issue; tooltip shows completion and assignee |
| Frontier | `state=OPEN` and every `blockedBy` node is resolved | Bright pulsing star with open orbit/ring; strongest label priority | Keyboard-focusable and clickable; “ready for discussion/work” text |
| Blocked decision | `state=OPEN` with at least one unresolved `blockedBy` node | Dim star behind a translucent dust lane; prerequisite edges point inward | Tooltip lists unresolved blockers as links |
| Blocker | An unresolved node with one or more outgoing `blocking` edges | Angular warning corona; outgoing routes use warm high-contrast dashes | Tooltip states how many nodes it blocks |
| Map | Label `wayfinder:map` or a root with sub-issues | Named constellation/sector boundary; completion arc around sector title | Select to frame its entire subgraph |
| Fog | Derived space beyond a map’s specified nodes, or around blocked descendants whose details are intentionally not yet specified | Soft mask/nebula with no fake star; “unmapped” label only at sufficient zoom | Non-clickable; never masquerades as a task |
| Out of scope / later | Issue outside the active map hierarchy, `NOT_PLANNED`, `wontfix`, or an explicit future-scope convention | Distant grey-blue belt separated by a boundary; no route into current frontier | Can open issue, but clearly says “outside current map” |
| Research | `wayfinder:research` | Telescope/scan-line halo; thin dotted orbit | Opens research ticket/report link from issue |
| Grilling / human decision | `wayfinder:grilling` | Split halo or paired star motif | Indicates human decision required |
| Prototype | `wayfinder:prototype` | Wireframe hex/star | Indicates experiment, not committed architecture |
| Task | `wayfinder:task` | Small satellite node | Lower label priority; groups around owning decision |

### Edge grammar

- **Hierarchy/sub-issue:** thin neutral constellation line from map/decision to child.
- **Dependency:** directed route from prerequisite to dependent, with a small arrow or moving dash only on focus; avoid continuous animation across the whole chart.
- **Resolved dependency:** low-opacity solid line.
- **Blocking dependency:** brighter dashed line crossing the fog boundary.
- **Cross-map relationship:** long curved route, visually distinct from parentage.

Hierarchy and dependency must never share identical line treatment. Otherwise users will confuse “part of this map” with “must happen first.”

### Layout grammar

Use a 2D chart, not 3D. The star-chart metaphor needs navigability and atmosphere, not perspective distortion.

1. Place each `wayfinder:map` root as a sector anchor.
2. Lay out its sub-issues radially or as a shallow spiral by dependency rank.
3. Keep resolved work inward/behind the current frontier; put reachable frontier stars on the sector’s bright rim; place blocked nodes beyond the rim in fog.
4. Put convergence gates such as `#13` at the far end of the sector where incoming dependency routes visibly meet.
5. Place unparented future issues in named outer regions, sorted deterministically by issue number until they acquire maps.
6. Freeze computed coordinates in the generated artifact for a given issue snapshot so stars do not drift on every render.

## 4. Read-only architecture

### Recommended pipeline

```text
GitHub Issues (canonical)
        |
        | read-only GraphQL query, cursor pagination
        v
normalizer + relation validator
        |
        +--> wayfinder.json (generated build artifact)
        |
        +--> Mermaid fallback (generated Markdown/SVG snapshot)
        |
        v
D3/SVG static site --> GitHub Pages
```

The build should fail visibly—not silently invent edges—when it sees a missing issue, relationship cycle that violates the chosen layout, duplicate node ID, inaccessible cross-repository dependency, or pagination truncation.

GitHub recommends avoiding frequent polling, using conditional requests/caching where applicable, and respecting rate-limit headers ([REST best practices](https://docs.github.com/en/rest/using-the-rest-api/best-practices-for-using-the-rest-api), [REST rate limits](https://docs.github.com/en/rest/using-the-rest-api/rate-limits-for-the-rest-api)). A build on push/manual dispatch plus a modest scheduled refresh is sufficient. Live browser polling provides little value for a design-planning chart.

GitHub Pages supports custom Actions workflows that build and deploy an artifact ([Pages custom workflows](https://docs.github.com/en/pages/getting-started-with-github-pages/using-custom-workflows-with-github-pages)). A build-time fetch permits the workflow’s `GITHUB_TOKEN` to remain server-side. No personal access token should appear in JavaScript, generated JSON, logs, source maps, or Pages configuration.

### Security controls

1. Declare least-privilege workflow permissions. The fetch/build job needs only repository contents and issue metadata read access; deployment permissions should be isolated to the deploy job.
2. Pin third-party Actions to full commit SHAs. GitHub identifies this as the immutable way to consume an Action and recommends least-privilege tokens ([Actions secure-use reference](https://docs.github.com/en/actions/reference/security/secure-use)).
3. Pin JavaScript dependencies with a committed lockfile. Enable the dependency graph and Dependabot alerts; GitHub notes that lockfiles improve direct and transitive dependency accuracy ([dependency graph](https://docs.github.com/en/code-security/concepts/supply-chain-security/dependency-graph-data), [Dependabot alerts](https://docs.github.com/en/code-security/how-tos/secure-your-supply-chain/secure-your-dependencies/configure-dependabot-alerts)).
4. Treat all issue strings as untrusted. Use DOM text nodes, allow only `https://github.com/...` issue URLs from the adapter, and do not render issue-body HTML in the MVP.
5. Use a restrictive Content Security Policy when hosting. Avoid runtime CDNs; bundle reviewed, pinned dependencies locally.
6. If Mermaid is self-hosted, use a patched release and strict security settings. Mermaid’s own repository warns that stored user-authored diagram text can contain malicious scripts, and its 2026 releases include security fixes ([Mermaid security discussion](https://github.com/mermaid-js/mermaid#security-and-safe-diagrams), [releases](https://github.com/mermaid-js/mermaid/releases)). GitHub-rendered Mermaid is safer operationally because GitHub owns that rendering boundary.
7. Do not adopt an unlicensed visual-reference repository. Public source is not automatically reusable open source.

### Licensing obligations

The recommended dependencies are permissive, but their notices must ship with distributions:

- D3’s ISC notice must be retained in copies ([D3 license](https://github.com/d3/d3/blob/main/LICENSE)).
- Mermaid, Cytoscape.js, Sigma, Graphology, PixiJS, and React Flow use MIT licenses that require preservation of copyright and permission notices in copies or substantial portions.
- d3-celestial and rot.js use BSD-3-Clause; that license adds a non-endorsement condition. Their code is not needed for the MVP.
- React Flow’s core is MIT, but Pro examples/templates are separately licensed. Do not copy the Pro auto-layout implementation merely because its public demo is visible.
- Any copied icons, fonts, star catalogues, textures, or example artwork require separate provenance checks. A library’s code license does not automatically license third-party demo assets.

## 5. Finite MVP and acceptance gates

The prototype should stop after proving the data and visual grammar. It should not become a second project manager.

### Included

- One repository: `coffeecentaur-afk/apogean`.
- All issues reachable from `wayfinder:map` roots, plus a limited outer belt of unparented future issues.
- Parent/sub-issue and blocked-by/blocking relationships.
- Resolved, frontier, blocked, blocker, map, fog, and out-of-scope visual states.
- Pan, zoom, fit-map, keyboard traversal, issue links, a legend, and a reduced-motion mode.
- Deterministic layout and a generated Mermaid fallback.
- Build timestamp and an explicit stale/error state.

### Excluded

- Editing issues, labels, assignees, dependencies, or positions from the chart.
- Authentication in the browser.
- Comments, full issue-body Markdown, discussions, pull requests, or real-time updates.
- 3D flight, procedural galaxies, particle-heavy effects, audio, or a game-like FOV simulation.
- Cross-repository aggregation until the one-repository adapter is complete and tested.

### Acceptance criteria

1. The graph exactly matches GitHub’s current hierarchy and dependency counts after full pagination.
2. `#2` and `#3` render resolved; `#4`–`#8` render as the current frontier; downstream `#9`–`#13` render blocked according to their unresolved prerequisites.
3. `#1` frames as a single named map, while `#14`–`#16` remain visually outside that map unless relationships later place them inside it.
4. Every visible issue can be reached by keyboard and opens its canonical GitHub URL.
5. Meaning remains understandable in grayscale and with animation disabled.
6. No GitHub credential or issue-body HTML reaches the browser bundle.
7. The same source snapshot produces the same coordinates and screenshot.
8. The dependency lockfile, license notices, and generated-data timestamp are present.

## Final recommendation

Proceed later with a narrow D3/SVG proof of concept backed by a build-time GraphQL adapter and a generated Mermaid fallback. Do not begin with Sigma, Pixi, 3D, or a standalone star-map codebase. The project’s immediate problem is faithfully exposing decisions and prerequisites, not rendering millions of particles.

GitHub Issues can remain the canonical Wayfinder database. The chart should be a disposable, reproducible read model: if it is deleted, it can be rebuilt from issue hierarchy, dependencies, labels, assignees, and state without losing project knowledge. That is the architectural property that prevents the visual Wayfinder from becoming another system the project must manually keep in sync.
