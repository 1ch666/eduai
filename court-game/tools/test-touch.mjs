import test from 'node:test';
import assert from 'node:assert/strict';
import { readFile } from 'node:fs/promises';
import { runInNewContext } from 'node:vm';
const source = await readFile(new URL('../Assets/WebGLTemplates/Court/touch-controls.js', import.meta.url), 'utf8');
function setup(mobile = true) {
  const sent = [], events = {}, nodes = {};
  function node(id) {
    return nodes[id] ||= { hidden: true, style: {}, listeners: {}, children: [],
      addEventListener(t, f) { this.listeners[t] = f; }, setPointerCapture() {},
      getBoundingClientRect: () => id === 'touch-stick' ? {left:0,top:500,width:128,height:128} : {left:0,top:44,width:400,height:700},
      replaceChildren() { this.children = []; }, appendChild(c) { this.children.push(c); } };
  }
  const window = { matchMedia: () => ({matches:mobile}), location: {search:''}, addEventListener: (t,f) => events[t] = f };
  runInNewContext(source, { window, navigator:{maxTouchPoints:mobile?2:0}, URLSearchParams,
    document: { getElementById:node, createElement: () => node(Math.random()),
      body:{classList:{add(){}}}, addEventListener: (t,f) => events[t] = f },
    setTimeout: () => 1, clearTimeout() {} });
  const controls = window.CourtTouch({ canvas:node('game'), getInstance:() => ({SendMessage:(...args) => sent.push(args)}), isEntered:() => true });
  controls.start();
  const fire = (id, type, x, y, pointerId = 1, timeStamp = 100) => nodes[id].listeners[type]({clientX:x,clientY:y,pointerId,timeStamp,preventDefault(){}});
  return {sent,nodes,window,events,controls,fire};
}
test('Desktop creates no touch UI or Unity calls', () => {
  const page = setup(false); assert.equal(page.controls.enabled,false); assert.deepEqual(page.sent,[]);
});
test('Joystick clamps diagonals and stops on cancellation / background', () => {
  const p=setup(); p.fire('touch-stick','pointerdown',300,300);
  let [x,y]=p.sent.at(-1)[2].split(',').map(Number); assert(Math.hypot(x,y)<=1.00001);
  p.fire('touch-stick','pointercancel',300,300); assert.equal(p.sent.at(-1)[2],'0,0');
  p.events.blur(); assert.equal(p.sent.at(-1)[1],'ResetTouchInput');
});
test('Second finger looks while joystick remains captured; dragging never interacts', () => {
  const p=setup(); p.fire('touch-stick','pointerdown',64,520,1);
  p.fire('touch-look','pointerdown',300,300,2);
  p.fire('touch-look','pointermove',340,300,2);
  assert.equal(p.sent.at(-1)[1],'TouchLook');
  p.fire('touch-look','pointerup',340,300,2,200);
  assert(!p.sent.some(x=>x[1]==='TouchInteract'));
  p.fire('touch-stick','pointermove',64,610,1); assert.equal(p.sent.at(-1)[1],'TouchMove');
});
test('Tap maps CSS canvas position to Unity bottom-origin viewport; canceled taps do nothing', () => {
  const p=setup(); p.fire('touch-look','pointerdown',100,219);
  p.fire('touch-look','pointerup',100,219,1,200);
  assert.deepEqual(p.sent.at(-1),['Player','TouchInteract','0.25,0.75']);
  const count=p.sent.length;
  p.fire('touch-look','pointerdown',100,219); p.fire('touch-look','pointercancel',100,219);
  p.fire('touch-look','pointerup',100,219,1,200); assert.equal(p.sent.length,count);
});
test('Answers use safe text, stop movement and block scene gestures', () => {
  const p=setup(); p.window.courtTouchChoices(['<img onerror=x>','B','C','D']);
  const buttons=p.nodes['touch-options'].children;
  assert.equal(buttons.length,4); assert.equal(buttons[0].textContent,'<img onerror=x>');
  assert.equal(p.nodes['touch-message'].style.visibility,'hidden');
  assert.equal(p.sent.at(-1)[1],'ResetTouchInput');
  buttons[2].listeners.click(); assert.deepEqual(p.sent.at(-1),['Canvas','ChooseFromTouch',2]);
  const count=p.sent.length; p.fire('touch-stick','pointerdown',60,510); assert.equal(p.sent.length,count);
  p.window.courtTouchChoices([]); assert.equal(p.nodes['touch-choices'].hidden,true);
  assert.equal(p.nodes['touch-message'].style.visibility,'visible');
});
