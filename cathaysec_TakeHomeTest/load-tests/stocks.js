import http from 'k6/http';
import { check, sleep } from 'k6';

const baseUrl = __ENV.BASE_URL || 'http://localhost:5155';
const apiKey = __ENV.API_KEY || 'cathaysec-dev-key';

export const options = {
  stages: [
    { duration: '15s', target: Number(__ENV.VUS || 20) },
    { duration: '30s', target: Number(__ENV.VUS || 20) },
    { duration: '15s', target: 0 },
  ],
  thresholds: {
    http_req_failed: ['rate<0.01'],
    http_req_duration: ['p(95)<500'],
  },
};

export default function () {
  const params = { headers: { 'X-API-Key': apiKey } };
  const list = http.get(`${baseUrl}/api/v1/stocks?page=1&pageSize=10`, params);
  check(list, { 'stock list is 200': (response) => response.status === 200 });

  const detail = http.get(`${baseUrl}/api/v1/stocks/2330`, params);
  check(detail, { 'stock detail is 200': (response) => response.status === 200 });
  sleep(0.2);
}
