import http, { file } from 'k6/http';
import { exec } from 'k6/execution';
import { check, sleep } from 'k6';
import { SharedArray } from 'k6/data';
import { htmlReport } from "https://raw.githubusercontent.com/benc-uk/k6-reporter/main/dist/bundle.js";
import { textSummary } from "https://jslib.k6.io/k6-summary/0.0.1/index.js";

// ========== INIT STAGE ========== //
const FILENAMES = [
    'sample0.dwg',
    'sample1.dwg',
    'sample2.dwg'
];


// Preload all files during init stage
const FILES = FILENAMES.map(name => ({
    name,
    content: open(`samples/${name}`, 'b')
}));


export default function () {

    // Pick a random file from preloaded array
    const fileObj = FILES[Math.floor(Math.random() * FILES.length)];

    console.log(`VU ${__VU} using ${fileObj.name}`);

    // 1. Upload
    const uploadRes = http.post('http://localhost:5000/api/drawings/upload', {
        file: http.file(fileObj.content, fileObj.name)
    });

    check(uploadRes, {
        'upload status 200': (r) => r.status === 200,
        'jobId returned': (r) => JSON.parse(r.body).jobId !== undefined
    });

    const jobId = JSON.parse(uploadRes.body).jobId;
    console.log(`VU ${__VU} uploading ${fileObj.name}, jobId: ${jobId}`);
    // 2. Status Polling
    let status = 'Processing';
    let attempts = 0;
    while ((status === 'Processing' || status === 'Pending') && attempts < 30) {
        sleep(2);
        const statusRes = http.get(`http://localhost:5000/api/drawings/status/${jobId}`);
        status = JSON.parse(statusRes.body)?.status || 'Undefined';
        attempts++;
    }

    // 3. Result Processing
    if (status === 'Completed') {
        const resultRes = http.get(`http://localhost:5000/api/drawings/result/${jobId}`);
        check(resultRes, {
            'result status 200': (r) => r.status === 200,
            'valid result format': (r) => r.json().output?.layers && r.json().output?.blocks
        });
    }
}

//load test scenario where 10 virtual users (VUs) are continuously active for 3 minutes//
export let options = {
    scenarios: {
        load_test: {
            executor: 'constant-vus',
            vus: 10,
            duration: '1m'
        }
    },
    thresholds: {
        http_req_duration: ['p(95)<5000'],
        checks: ['rate>0.95']
    }
};

// ========== SUMMARY STAGE ========== //
export function handleSummary(data) {
    return {
        "result.html": htmlReport(data),
        stdout: textSummary(data, { indent: " ", enableColors: true }),
    };
}
