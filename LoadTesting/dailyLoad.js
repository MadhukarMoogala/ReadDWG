import http from 'k6/http';
import { check, sleep } from 'k6';

// Single file for consistent daily monitoring
const dwgFile = open('samples/sample0.dwg', 'b');

export let options = {
    scenarios: {
        background_load: {
            executor: 'constant-arrival-rate',
            rate: 300,
            timeUnit: '24h',
            duration: '24h',
            preAllocatedVUs: 2,
            maxVUs: 5
        }
    },
    thresholds: {
        http_req_duration: ['p(95)<10000'], // Relaxed for 24h window
        checks: ['rate>0.95']
    }
};

export default function () {
    const uploadRes = http.post('http://localhost:5000/api/drawings/upload', {
        file: http.file(dwgFile, 'daily-check.dwg')
    });

    check(uploadRes, {
        'upload OK': (r) => r.status === 200,
        'has jobId': (r) => JSON.parse(r.body).jobId
    });

    // Minimal status check - daily tests don't need full validation
    sleep(1);
}

// k6 run daily-load.js --duration 10m 