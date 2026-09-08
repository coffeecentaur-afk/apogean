# A5 pod-only mask

Built-in image tool; precise-object-edit. Input/edit target: ../A5-ImpactReview/design-a5-impact.png (SHA256 3EEC46AFFC7BB149B6A38528AFB93ABB79E27D24E61D97995A5A18768AEB1C67).

Produce only a black-and-white selection mask for the provided image. Keep the exact 1536 x 1024 canvas and the exact location, scale, silhouette and proportions of the pod. Do not recenter, enlarge, crop, redraw, or add anything. Solid opaque pure white means KEEP; solid opaque pure black means REMOVE. No gray, texture, labels, shading, transparency, or antialiasing.

Keep ONLY the crashed pod: broken antenna/roof, main capsule shell, dark interior including its seat, open right hatch, both hinges and the short metallic foot/ramp at the bottom. Treat the dark cockpit/seat/hatch interiors as WHITE, not holes. Include the pod's metal feet/base supports, with dirt attached directly to their surface, but not the surrounding earth. Exclude the entire background and the whole impact crater, detached ground fragments, grasses, shadows on the ground, and soil below/beside the pod. The short center ramp ends around y=836. Do not include a flat ground strip beneath the base. Leave black in genuine exterior gaps. Preserve all physically attached hardware.

This is a coordinate-locked extraction proposal, not new artwork. The source colors will be retained by the deterministic exporter; this mask supplies selection only. Its alignment and final export must be inspected before acceptance.
