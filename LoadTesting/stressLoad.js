import http from 'k6/http';
import { check } from 'k6';

export let options = {
    scenarios: {
        spike_test: {
            executor: 'ramping-vus',
            stages: [
                { duration: '2m', target: 50 },  // Fast ramp-up
                { duration: '5m', target: 100 },  // Sustain load
                { duration: '2m', target: 0 }     // Cool down
            ]
        }
    },
    thresholds: {
        http_req_failed: ['rate<0.05'], // Allow 5% errors during stress
        http_req_duration: ['p(95)<15000']
    }
};

const dwgFile = open('samples/sample0.dwg', 'b');

export default function () {
    const res = http.post('http://localhost:5000/api/drawings/upload', {
        file: http.file(dwgFile, 'stress-test.dwg')
    });

    check(res, { 'accepted request': (r) => r.status === 202 }); // 202 Accepted may be expected during stress
}

//k6 cloud stress-test.js