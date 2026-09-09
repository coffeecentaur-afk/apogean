# Failed partial export — do not consume

Short passed. Long aborted because PowerShell converted its absent source-mask
string to empty. The sampler now treats null and empty as absent. This partial
run is retained as failure evidence, not a family candidate or release.
Use the separately checked `../Native-v1/` for review.
