import os

os.environ["OPENAPI_API_KEY"] = "sk-dummy-test-key-for-ci"

from fastapi.testclient import TestClient
from main import app

client = TestClient(app)

def test_health_check():
    response = client.get("/docs")
    assert response.status_code == 200