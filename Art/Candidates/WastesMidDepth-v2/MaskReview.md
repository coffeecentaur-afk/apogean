# Lower-mask and footing review

Agent reviewed the original lower sources, full prepared candidates, join board
and light/dark composites before the QA build. This is permission to test the
candidate, not a user art verdict.

The approved upper masks are reused unchanged through module row440. Lower
exterior mask scans inward from each image side until material is reached:
neutral painted matte for Station/garage/checkpoint and black exterior for
shell. This scan never traverses a building window. Internal openings in the
protected architecture retain the approved upper's exact alpha.

`Proposal/` exposed pale neutral fringe. `EdgeReview/` retains that silhouette
but prepares only selected edge RGB: neutral/light candidates within2px of
clear exterior use a darker warm-material donor no more than6px away. Alpha is
unchanged. No global erosion, blanket beige recolor, or building repaint.
Every changed pixel and donor is recorded and independently rechecked.

| Asset | Protected upper pixels | Lower-source kept pixels | RGB donors |
| --- | ---: | ---: | ---: |
| Station | 225792 | 428414 | 892 |
| MotorDepot | 225792 | 392422 | 580 |
| BrokenShell | 225792 | 406787 | 0 |
| Checkpoint | 225792 | 246201 | 594 |

All720896 final pixels per module pass source/selector/export comparison.
Binary alpha, RGB0 under alpha0, and lower support are structural gates only.
Light/dark review still shows occasional small pale lower-edge pixels and some
similar Station/garage geology. Those are disclosed polish targets; “zero
checkerboard” is not a promise that every contour is artistically final.
