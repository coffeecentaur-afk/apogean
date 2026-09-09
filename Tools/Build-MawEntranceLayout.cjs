'use strict';
// Candidate-only tile geometry. No tModLoader dependency, asset or world writes.
const fs = require('node:fs');
const path = require('node:path');
const assert = require('node:assert/strict');
const candidate = path.join(__dirname, '../Art/Candidates/MawEntrance-v1');
const layout = JSON.parse(fs.readFileSync(path.join(candidate, 'entrance-layout.json'), 'utf8'));

function inside(x, y, polygon) {
  let hit = false;
  for (let i = 0, j = polygon.length - 1; i < polygon.length; j = i++) {
    const [xi, yi] = polygon[i], [xj, yj] = polygon[j];
    if ((yi > y) !== (yj > y) && x < (xj - xi) * (y - yi) / (yj - yi) + xi) hit = !hit;
  }
  return hit;
}

function validate(spec) {
  const { width: w, height: h } = spec;
  assert.equal(spec.schemaVersion, 2);
  assert.ok(Number.isInteger(w) && Number.isInteger(h) && w > 0 && h > 0 && w * h <= 20000);
  for (const polygon of spec.terrain) {
    for (const [x, y] of polygon) assert.ok(Number.isInteger(x) && Number.isInteger(y) && x >= 0 && x <= w && y >= 0 && y <= h, 'Terrain vertex outside candidate');
  }
  const rimHeights=spec.terrain.slice(0,2).map(p=>spec.groundY-Math.min(...p.map(v=>v[1])));
  assert.ok(rimHeights.every(rise=>rise>=16), 'Raised mouth rim must remain substantially above ground');
  const banks=spec.terrain.map(p=>Array.from({length:h},(_,y)=>Array.from({length:w},(_,x)=>inside(x+.5,y+.5,p))));
  const grid = Array.from({length: h}, (_, y) => Array.from({length: w}, (_, x) => banks.some(b=>b[y][x]) ? 1 : 0));
  const at = (x, y) => grid[y]?.[x];
  const neighbors = (x,y) => [[x-1,y],[x+1,y],[x,y-1],[x,y+1]];
  const inBounds = (x,y) => x >= 0 && x < w && y >= 0 && y < h;
  // Derive attachment from this exact candidate's terrain, not a second
  // hand-painted diagram. These rectangles are footprints, not final sprites.
  const teeth=structuredClone(spec.teeth);
  for (const band of spec.wallTeeth) {
    assert.ok([0,1].includes(band.bank) && Number.isInteger(band.spacing) && band.spacing>=4);
    assert.ok(Number.isInteger(band.fromY) && band.fromY>=0 && Number.isInteger(band.toY) && band.toY+2<h);
    assert.ok(band.lengths.length && band.lengths.every(n=>Number.isInteger(n) && n>=3 && n<=12));
    for(let y=band.fromY,n=0;y<=band.toY;y+=band.spacing,n++) {
      const length=band.lengths[n%band.lengths.length];
      const widths=[Math.ceil(length*.7),length,Math.ceil(length*.4)];
      const rects=widths.map((width,dy)=>{
        const row=banks[band.bank][y+dy];
        const edge=band.bank===0 ? row.lastIndexOf(true)+1 : row.indexOf(true);
        assert.ok(edge>0 && edge<w,'Missing tooth wall anchor');
        return [band.bank===0 ? edge : edge-width,y+dy,width,1];
      });
      teeth.push(rects);
    }
  }
  for(const crown of spec.crownTeeth) {
    assert.ok([0,1].includes(crown.bank) && Number.isInteger(crown.x) && crown.x>=0 && crown.x+crown.heights.length<=w);
    teeth.push(crown.heights.map((height,dx)=>{
      assert.ok(Number.isInteger(height) && height>0);
      const x=crown.x+dx, top=banks[crown.bank].findIndex(row=>row[x]);
      assert.ok(top>=height,'Crown tooth has no support or exceeds preview bounds');
      return [x,top-height,1,height];
    }));
  }
  assert.ok(teeth.length>=40,'Dense teeth contract: sparse tooth count rejected');
  for (const tooth of teeth) {
    const cells = new Set();
    for (const [x,y,rw,rh] of tooth) {
      assert.ok([x,y,rw,rh].every(Number.isInteger) && rw > 0 && rh > 0, 'Invalid tooth rectangle');
      for (let dy = 0; dy < rh; dy++) for (let dx = 0; dx < rw; dx++) {
        const xx = x + dx, yy = y + dy;
        assert.ok(inBounds(xx,yy) && at(xx,yy) === 0, 'Tooth overlaps terrain, another tooth or boundary');
        grid[yy][xx] = 2;
        cells.add(yy*w+xx);
      }
    }
    const queue = [cells.values().next().value], reached = new Set(queue);
    let attached = false;
    for (let i=0; i<queue.length; i++) {
      const n=queue[i], x=n%w, y=Math.floor(n/w);
      for (const [nx,ny] of neighbors(x,y)) {
        if (at(nx,ny) === 1) attached=true;
        const k=ny*w+nx;
        if (inBounds(nx,ny) && cells.has(k) && !reached.has(k)) { reached.add(k); queue.push(k); }
      }
    }
    assert.ok(attached && reached.size === cells.size, `Tooth is floating or disconnected: ${JSON.stringify(tooth)}`);
  }
  const pools = [];
  for (const basin of spec.basins) {
    const [sx,sy]=basin.seed;
    assert.ok(inBounds(sx,sy) && sy >= basin.surfaceY && at(sx,sy) === 0, 'Invalid basin seed');
    const queue=[[sx,sy]], seen=new Set([sy*w+sx]);
    for (let i=0; i<queue.length; i++) {
      const [x,y]=queue[i];
      assert.ok(x > 0 && x < w-1 && y < h-1, 'Basin leaks to candidate boundary');
      grid[y][x]=3;
      for (const [nx,ny] of neighbors(x,y)) {
        const k=ny*w+nx;
        if (ny >= basin.surfaceY && inBounds(nx,ny) && at(nx,ny) === 0 && !seen.has(k)) { seen.add(k); queue.push([nx,ny]); }
      }
    }
    pools.push({id:basin.id, cells:queue.length});
  }
  assert.ok(pools[0].cells > pools[1].cells, 'Left basin must be larger');
  function clear(box) {
    for (let y=box.y; y<box.y+box.height; y++) for (let x=box.x; x<box.x+box.width; x++) if (at(x,y) !== 0) return false;
    return true;
  }
  // The outside surface supplies the scale figure's starting point, not a
  // generated resting shelf inside the descent.
  for (const box of [spec.player]) {
    assert.ok(clear(box), 'Dry standing volume obstructed');
    for (let x=box.x;x<box.x+box.width;x++) assert.equal(at(x,box.y+box.height),1,'Dry landing lacks safe support');
  }
  assert.ok(!spec.dryLandings?.length, 'No generated safe ledges: remove reserved resting positions');
  const window=spec.descentWindow;
  assert.ok(window && window.top>=3 && window.top<h && window.left>=0 && window.right<w);
  for (let y=window.top; y<h; y++) for (let x=window.left; x<window.right; x++) {
    assert.ok(!(at(x,y)===1 && at(x+1,y)===1 && clear({x,y:y-3,width:2,height:3})),
      `No generated safe ledges: two-tile resting shelf at ${x},${y}`);
  }
  assert.ok(clear(spec.exit), 'Exit obstructed');
  const visited=new Set(), queue=[[spec.player.x,spec.player.y]];
  visited.add(spec.player.y*w+spec.player.x);
  for (let i=0;i<queue.length;i++) {
    for (const [x,y] of neighbors(...queue[i])) {
      const k=y*w+x;
      if (x<0 || y<0 || x+2>w || y+3>h || visited.has(k) || !clear({x,y,width:2,height:3})) continue;
      visited.add(k); queue.push([x,y]);
    }
  }
  assert.ok(visited.has(spec.exit.y*w+spec.exit.x), 'No dry 2x3 clearance route to exit');
  // Route dots are diagram annotations, not a claim about jumps/fall survivability.
  for (let i=1;i<spec.route.length;i++) {
    const [ax,ay]=spec.route[i-1], [bx,by]=spec.route[i];
    const steps=Math.ceil(Math.hypot(bx-ax,by-ay)*4);
    for (let t=0;t<=steps;t++) assert.equal(at(Math.floor(ax+(bx-ax)*t/steps),Math.floor(ay+(by-ay)*t/steps)),0,'Diagram route crosses a hazard or solid');
  }
  const runs=[];
  for (let y=0;y<h;y++) for (let x=0;x<w;) {
    const kind=at(x,y), start=x;
    while (x<w && at(x,y)===kind) x++;
    if (kind) runs.push([start,y,x-start,kind]);
  }
  return {runs, pools, reachableStandingPositions:visited.size, teeth:teeth.length, rimHeights};
}

const proof=validate(layout);
for (const [name,mutate] of [
  ['floating tooth', s=>s.teeth.push([[61,7,1,1]])],
  ['blocked entrance', s=>s.player.x=10],
  ['restored safe ledge', s=>s.terrain.push([[60,70],[65,70],[65,72],[60,72]])],
  ['lowered rim', s=>s.terrain[0]=s.terrain[0].map(([x,y])=>[x,Math.max(y,s.groundY-4)])],
  ['sparse teeth', s=>s.wallTeeth=[]],
  ['leaking pool', s=>s.basins[0].surfaceY=0],
  ['blocked exit', s=>s.exit.x=0],
  ['false route annotation', s=>s.route[2]=[0,35]],
  ['out-of-bounds geometry', s=>s.terrain[0][0]=[-1,18]]
]) {
  const broken=structuredClone(layout); mutate(broken);
  const expected={'restored safe ledge':/No generated safe ledges/,'lowered rim':/Raised mouth rim/,'sparse teeth':/Dense teeth/}[name];
  assert.throws(()=>validate(broken), expected, `Negative control did not fail: ${name}`);
}
console.log(JSON.stringify({status:'PASS: candidate geometry only', width:layout.width,height:layout.height,pools:proof.pools,teeth:proof.teeth,rimHeightsAboveGround:proof.rimHeights,reachableStandingPositions:proof.reachableStandingPositions,generatedRestingShelves:0,negativeControls:9, nativeGameplayTested:false},null,2));
const args=process.argv.slice(2);
if (args.length) {
  assert.ok(args.length===2 && args[0]==='--preview', 'Usage: node Tools/Build-MawEntranceLayout.cjs [--preview absolute-output.html]');
  assert.ok(path.isAbsolute(args[1]) && path.extname(args[1])==='.html','Use an absolute HTML output path');
  const template=fs.readFileSync(path.join(candidate,'layout-preview.template.html'),'utf8');
  assert.equal(template.split('__MAW_LAYOUT__').length,2);
  const html=template.replace('__MAW_LAYOUT__',JSON.stringify({...layout,runs:proof.runs,toothCount:proof.teeth,rimHeights:proof.rimHeights}));
  assert.ok(Buffer.byteLength(html)<1000000);
  fs.mkdirSync(path.dirname(args[1]),{recursive:true});
  fs.writeFileSync(args[1],html);
  console.log(`Preview: ${args[1]}`);
}
