import test from 'node:test';
import assert from 'node:assert/strict';
import { readFile } from 'node:fs/promises';
import { runInNewContext } from 'node:vm';
const html = await readFile(new URL('../Assets/WebGLTemplates/Court/index.html', import.meta.url), 'utf8');
const script = html.match(/<script>([\s\S]*?)<\/script>/)[1];
const tick = () => new Promise(resolve => setImmediate(resolve));

function createPage(pointerLock) {
  const sent = [];
  const nodes = Object.fromEntries(['game', 'start', 'status', 'progress', 'error', 'cover', 'input-mode'].map(id =>
    [id, { listeners: {}, addEventListener(type, callback) { this.listeners[type] = callback; }, focus() {} }]));
  nodes.game.requestPointerLock = pointerLock;
  const documentEvents = {};
  runInNewContext(script, {
    document: { getElementById: id => nodes[id], createElement: () => ({}),
      addEventListener: (type, callback) => { documentEvents[type] = callback; },
      body: { appendChild: element => element.onload() } },
    window: { devicePixelRatio: 2 },
    createUnityInstance: async () => ({ SendMessage: (...args) => sent.push(args) })
  });
  return { nodes, sent, documentEvents };
}

test('Successful pointer lock does not enable fallback', async () => {
  const { nodes, sent } = createPage(() => Promise.resolve());
  nodes.start.listeners.click(); await tick();
  await nodes.game.requestPointerLock();
  assert.equal(sent.length, 0);
  assert.equal(nodes.start.textContent, '進入法庭');
});
test('Rejected pointer lock uses drag look once without unhandled rejection', async () => {
  const { nodes, sent, documentEvents } = createPage(() => Promise.reject(new Error('not supported')));
  nodes.start.listeners.click(); await tick();
  await nodes.game.requestPointerLock();
  documentEvents.pointerlockerror();
  assert.deepEqual(sent, [['Player', 'EnableDragLook']]);
  assert.match(nodes['input-mode'].textContent, /拖曳/);
});
test('Missing pointer lock API queues fallback until Unity is ready', async () => {
  const { nodes, sent } = createPage(undefined);
  nodes.game.requestPointerLock(); await tick();
  nodes.start.listeners.click(); await tick();
  assert.deepEqual(sent, [['Player', 'EnableDragLook']]);
});
