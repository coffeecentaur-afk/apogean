# Entity contracts

## Before art

Record:

- role and threat tier;
- frame canvas and frame count;
- occupied silhouette target;
- default facing;
- hitbox and draw offset;
- frame meanings and timings;
- palette budget;
- nearest vanilla size references.

## Static checks

- sheet height divides by frame count;
- every required frame is non-empty;
- occupied bounds meet the concept target;
- no accidental soft alpha;
- no frame crosses its cell;
- anchor drift stays within the intended motion budget;
- palette stays readable at 1x.

## Live checks

Use a deterministic gallery or arena that spawns the entity beside size references and exercises both directions, movement, attack, damage, death, item drop, lighting, and zoom. Save a dated screenshot before promotion.
