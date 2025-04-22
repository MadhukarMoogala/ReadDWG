import http, { file } from 'k6/http';
import { exec } from 'k6/execution';
import { check, sleep } from 'k6';
import { SharedArray } from 'k6/data';
import { textSummary } from 'https://jslib.k6.io/k6-summary/0.0.1/index.js';

// ========== INIT STAGE ========== //
const FILENAMES = [
    'sample0.dwg',
    'sample1.dwg',
    'sample2.dwg'
];
const fileName = FILENAMES[1];

const fileContent = open(`samples/${fileName}`, 'b');


export default function () {

    console.log(`VU ${__VU} starting test with file: ${fileName}`);
    console.log(`Iter ${__ITER} of ${__VU}`);
    // 1. File Upload
    const uploadRes = http.post('http://localhost:5000/api/drawings/upload', {
        file: http.file(fileContent, fileName)
    });

    check(uploadRes, {
        'upload status 200': (r) => r.status === 200,
        'jobId returned': (r) => JSON.parse(r.body).jobId !== undefined
    });

    const jobId = JSON.parse(uploadRes.body).jobId;
    console.log(`VU ${__VU} uploading ${fileName}, jobId: ${jobId}`);
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

// ========== CONFIG ========== //
export let options = {
    scenarios: {
        load_test: {
            executor: 'constant-vus',
            vus: 10,
            duration: '5m'
        }
    },
    thresholds: {
        http_req_duration: ['p(95)<5000'],
        checks: ['rate>0.95']
    }
};

export function handleSummary(data) {
    return {
        stdout: textSummary(data, { indent: ' ', enableColors: true })
    };
}

/*
const TEMP_DIR = '..\\Build\\WebApi\\Temp';

export function teardown() {
    console.log(`Wiping ${TEMP_DIR}...`);

    try {
        // PowerShell command to force-delete everything
        const result = exec('powershell', `Remove-Item -Path "${TEMP_DIR}\\*" -Recurse -Force`);
        console.log(`Cleanup completed. Exit code: ${result.exit_code}`);

        // Verify empty directory
        const verify = exec('powershell', `(Get-ChildItem "${TEMP_DIR}" | Measure-Object).Count`);
        if (parseInt(verify.stdout) === 0) {
            console.log("Verification: Directory is empty");
        } else {
            console.error(`Verification failed! Files remaining: ${verify.stdout}`);
        }

    } catch (e) {
        console.error('Cleanup failed:', e);
        file.append('cleanup_errors.log', JSON.stringify(e) + '\n');
    }
}*/