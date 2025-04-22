import http from 'k6/http';
import { check, sleep } from 'k6';
import { Trend } from 'k6/metrics';
import { textSummary } from 'https://jslib.k6.io/k6-summary/0.0.1/index.js';

const processingTime = new Trend('processing_time_ms');
const files = ['sample0.dwg', 'sample1.dwg']; // 2 sample files

export let options = {
    scenarios: {
        workday_peak: {
            executor: 'ramping-arrival-rate',
            startRate: 5,      // Start with 5 reqs/minute
            timeUnit: '1m',
            stages: [
                { target: 10, duration: '30m' }, // Ramp up
                { target: 15, duration: '4h' },   // Core hours
                { target: 5, duration: '30m' }    // Ramp down
            ],
            preAllocatedVUs: 3,
            maxVUs: 20
        }
    }
};

export default function () {
    const start = Date.now();
    const file = files[__ITER % 2];
    const dwgFile = open(`samples/${file}`, 'b');

    const uploadRes = http.post('http://localhost:5000/api/drawings/upload', {
        file: http.file(dwgFile, file)
    });

    const jobId = JSON.parse(uploadRes.body).jobId;

    // Light status polling
    let status = 'Processing';
    while (status === 'Processing') {
        sleep(2);
        const statusRes = http.get(`http://localhost:5000/api/drawings/status/${jobId}`);
        status = JSON.parse(statusRes.body).Status;
    }

    processingTime.add(Date.now() - start);
}
//k6 run business-hours.js