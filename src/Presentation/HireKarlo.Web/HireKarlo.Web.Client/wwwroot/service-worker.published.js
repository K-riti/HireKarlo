// This is the published service worker - more aggressive caching
const CACHE_NAME = 'hirekarlo-v1';

// Precache assets from the service-worker-assets.js manifest
self.addEventListener('install', event => {
    event.waitUntil(
        caches.open(CACHE_NAME)
            .then(cache => {
                return cache.addAll(self.assetsManifest.assets.map(asset => asset.url));
            })
            .then(() => self.skipWaiting())
    );
});

self.addEventListener('activate', event => {
    event.waitUntil(
        caches.keys()
            .then(cacheNames => {
                return Promise.all(
                    cacheNames
                        .filter(name => name !== CACHE_NAME)
                        .map(name => caches.delete(name))
                );
            })
            .then(() => self.clients.claim())
    );
});

self.addEventListener('fetch', event => {
    if (event.request.method !== 'GET') return;
    if (event.request.url.includes('/api/')) return;

    event.respondWith(
        caches.match(event.request)
            .then(response => {
                if (response) {
                    // Update cache in background
                    fetch(event.request)
                        .then(fetchResponse => {
                            caches.open(CACHE_NAME)
                                .then(cache => cache.put(event.request, fetchResponse));
                        });
                    return response;
                }
                return fetch(event.request)
                    .then(response => {
                        if (!response || response.status !== 200) {
                            return response;
                        }
                        const responseToCache = response.clone();
                        caches.open(CACHE_NAME)
                            .then(cache => cache.put(event.request, responseToCache));
                        return response;
                    });
            })
    );
});

self.addEventListener('push', event => {
    const options = {
        body: event.data?.text() || 'New update from HireKarlo',
        icon: '/icon-192.png',
        badge: '/icon-192.png'
    };
    event.waitUntil(self.registration.showNotification('HireKarlo', options));
});

self.addEventListener('notificationclick', event => {
    event.notification.close();
    event.waitUntil(clients.openWindow('/'));
});
