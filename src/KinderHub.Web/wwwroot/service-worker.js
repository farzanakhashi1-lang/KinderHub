// KinderHub service worker (PWA)
// - Makes the app installable on phones and computers.
// - When there is no internet, shows a friendly offline page instead of the browser error.
// Data is NOT cached: KinderHub always shows live data from the server.
const CACHE = 'kinderhub-v1';
const OFFLINE_URL = 'offline.html';
const PRECACHE = [OFFLINE_URL, 'icon-192.png', 'icon-512.png'];

self.addEventListener('install', event => {
    event.waitUntil(caches.open(CACHE).then(cache => cache.addAll(PRECACHE)));
    self.skipWaiting();
});

self.addEventListener('activate', event => {
    event.waitUntil(
        caches.keys().then(keys => Promise.all(keys.filter(k => k !== CACHE).map(k => caches.delete(k))))
    );
    self.clients.claim();
});

self.addEventListener('fetch', event => {
    // Only page navigations get the offline fallback; everything else goes to the network as usual
    if (event.request.mode === 'navigate') {
        event.respondWith(fetch(event.request).catch(() => caches.match(OFFLINE_URL)));
    }
});
