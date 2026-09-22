// Mobile-only Pointer Events UI. Pointer IDs keep joystick + look independent.
// No synthetic keyboard events, external libraries, permissions or network calls.
window.CourtTouch = function ({ canvas, getInstance, isEntered }) {
  const enabled = (navigator.maxTouchPoints > 0 && window.matchMedia('(pointer: coarse)').matches)
    || new URLSearchParams(window.location.search).get('touch') === '1'; // Explicit QA preview only.
  if (!enabled) return { enabled: false, start() {} };
  const root = document.getElementById('touch-ui');
  const stick = document.getElementById('touch-stick');
  const knob = document.getElementById('touch-knob');
  const look = document.getElementById('touch-look');
  const modal = document.getElementById('touch-choices');
  const buttons = document.getElementById('touch-options');
  const message = document.getElementById('touch-message');
  let stickId = null, lookGesture = null, choicesOpen = false, messageTimer;
  const send = (method, value, object = 'Player') => {
    if (isEntered() && getInstance()) getInstance().SendMessage(object, method, value);
  };
  const stop = () => {
    stickId = null; lookGesture = null;
    knob.style.transform = 'translate(0px,0px)';
    send('ResetTouchInput');
  };
  const moveStick = e => {
    const rect = stick.getBoundingClientRect();
    const radius = rect.width * .36;
    let x = (e.clientX - rect.left - rect.width / 2) / radius;
    let y = (rect.top + rect.height / 2 - e.clientY) / radius;
    const magnitude = Math.hypot(x, y);
    if (magnitude > 1) { x /= magnitude; y /= magnitude; }
    if (magnitude < .12) x = y = 0;
    knob.style.transform = `translate(${x * radius}px,${-y * radius}px)`;
    send('TouchMove', `${x},${y}`);
  };
  stick.addEventListener('pointerdown', e => {
    if (!isEntered() || choicesOpen || stickId !== null) return;
    e.preventDefault(); stickId = e.pointerId; stick.setPointerCapture(e.pointerId); moveStick(e);
  });
  stick.addEventListener('pointermove', e => {
    if (e.pointerId !== stickId) return;
    e.preventDefault(); moveStick(e);
  });
  const releaseStick = e => {
    if (e.pointerId !== stickId) return;
    stickId = null; knob.style.transform = 'translate(0px,0px)'; send('TouchMove', '0,0');
  };
  for (const type of ['pointerup', 'pointercancel', 'lostpointercapture']) stick.addEventListener(type, releaseStick);
  look.addEventListener('pointerdown', e => {
    if (!isEntered() || choicesOpen || lookGesture) return;
    e.preventDefault(); look.setPointerCapture(e.pointerId);
    lookGesture = { id: e.pointerId, x: e.clientX, y: e.clientY, startX: e.clientX, startY: e.clientY,
      time: e.timeStamp, moved: false };
  });
  look.addEventListener('pointermove', e => {
    const g = lookGesture;
    if (!g || e.pointerId !== g.id) return;
    e.preventDefault();
    const wasMoving = g.moved;
    g.moved ||= Math.hypot(e.clientX - g.startX, e.clientY - g.startY) > 10;
    if (g.moved) {
      const rect = canvas.getBoundingClientRect();
      send('TouchLook', `${(e.clientX - (wasMoving ? g.x : g.startX)) / rect.width * 140},${((wasMoving ? g.y : g.startY) - e.clientY) / rect.height * 90}`);
    }
    g.x = e.clientX; g.y = e.clientY;
  });
  look.addEventListener('pointerup', e => {
    const g = lookGesture;
    if (!g || e.pointerId !== g.id) return;
    lookGesture = null;
    if (!g.moved && Math.hypot(e.clientX - g.startX, e.clientY - g.startY) <= 10 && e.timeStamp - g.time < 500) {
      const rect = canvas.getBoundingClientRect();
      send('TouchInteract', `${(e.clientX - rect.left) / rect.width},${1 - (e.clientY - rect.top) / rect.height}`);
    }
  });
  for (const type of ['pointercancel', 'lostpointercapture']) look.addEventListener(type, e => {
    if (lookGesture?.id === e.pointerId) lookGesture = null;
  });
  window.addEventListener('blur', stop);
  window.addEventListener('resize', stop);
  document.addEventListener('visibilitychange', stop);
  window.courtTouchChoices = options => {
    stop(); choicesOpen = options.length > 0;
    document.getElementById('touch-question').textContent = message.textContent;
    message.style.visibility = choicesOpen ? 'hidden' : 'visible';
    modal.hidden = !choicesOpen; buttons.replaceChildren();
    for (const [index, text] of options.entries()) {
      const button = document.createElement('button');
      button.type = 'button'; button.textContent = text;
      button.addEventListener('click', () => {
        if (choicesOpen) send('ChooseFromTouch', index, 'Canvas');
      });
      buttons.appendChild(button);
    }
  };
  window.courtTouchMessage = (text, seconds) => {
    clearTimeout(messageTimer); message.textContent = text; message.hidden = !text;
    messageTimer = setTimeout(() => { message.hidden = true; }, Math.max(1, seconds) * 1000);
  };
  return { enabled: true, start() {
    document.body.classList.add('touch-mode'); root.hidden = false;
    send('EnableTouchControls');
    document.getElementById('input-mode').textContent = '搖桿移動 · 滑動轉向 · 點擊互動';
  } };
};
