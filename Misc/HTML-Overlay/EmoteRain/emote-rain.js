/**
 * Configuration options
 */
const CONFIG = {
    // The maximum number of emotes to display at once
    maxEmotes: 20,

    // The size of the emotes (css)
    emoteSize: '100px',

    // State of the emote-rain
    enabled: true
};

window.client.on('General.Custom', (payload) => {
    if (!payload) return;

    if (payload.data && payload.data.event === 'htmlOverlay_EmoteRain' && payload.data.config) {
        Object.entries(payload.data.config).forEach(([key, value]) => {
            if (key in CONFIG) CONFIG[key] = value;
        });
    }
});


// Streamer.bot Event Listener
window.client.on('Twitch.ChatMessage', (message) => {
    if (CONFIG.enabled) {
        console.log(`emotes: ${message.data.message.emotes.length}`);
        if (message.data.message.emotes.length > 0) {
            let emoteCount = Math.min(message.data.message.emotes.length, CONFIG.maxEmotes);

            for (let i = 0; i < emoteCount; i++) {
                console.log(message.data.message.emotes[i].imageUrl);
                setTimeout(() => {
                    emoteRain(message.data.message.emotes[i].imageUrl);
                }, 300);
            }
        }
    }
});

// GSAP deps
import { Linear, Sine, TweenLite, TweenMax } from 'https://cdn.skypack.dev/gsap/all';

// Create our confetti container and append to document body
const confettiContainer = document.createElement("div");
confettiContainer.id = 'confetti-container';
document.body.appendChild(confettiContainer);

// Set default values for perspective property
// TweenLite.set("#confetti-container", {perspective:600})
let idx = 0;
function emoteRain(imageUrl) {

  // resize container to window size
  confettiContainer.innerWidth = window.innerWidth;
  confettiContainer.innerHeight = window.innerHeight;

  const numElements = 1;

  // load new elements into the page
  for (let i = 0; i < numElements; i++) {
    const element = document.createElement('div');
    element.id = idx;
    idx++;

    TweenLite.set(element, {
      className:'falling-element',
      x: Randomizer(0, innerWidth),
      y: Randomizer(-500, -450),
      z: Randomizer(-200, 200)
    });

    // switch between the images.
    element.style.background= `url(${imageUrl})`;
    element.style.backgroundSize= '100% 100%';
    element.style.width = CONFIG.emoteSize;
    element.style.height = CONFIG.emoteSize;

    confettiContainer.appendChild(element);

    // run animation
    runFallingAnimation(element);

    setTimeout(() => {
      document.getElementById(element.id).remove();
    }, 15000);
  }
}

function runFallingAnimation(element) {
  TweenMax.to(element, Randomizer(6, 16), {
    y: window.innerHeight+1400,
    ease: Linear.easeNone,
    repeat: 0,
    delay: -1
  });
  TweenMax.to(element, Randomizer(4, 8), {
    x: '+=100',
    rotationZ: Randomizer(0, 180),
    repeat: 4,
    yoyo: true,
    ease: Sine.easeInOut
  });
  TweenMax.to(element, Randomizer(2, 8), {
    rotationX: Randomizer(0,360),
    rotationY: Randomizer(0,360),
    repeat: 8,
    yoyo: true,
    ease: Sine.easeInOut,
    delay: -5
  });
}

function Randomizer(min, max) { return min + Math.random()*(max-min); }
