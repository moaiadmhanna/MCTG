#!/bin/bash

# --------------------------------------------------
# Monster Trading Cards Game - CURL Testing
# --------------------------------------------------
echo "CURL Testing for Monster Trading Cards Game"
echo "Syntax: ./curl_script.sh [pause]"
echo " - pause: optional, if set, the script will pause after each block"
echo

# Check for pause flag
pauseFlag=0
for arg in "$@"; do
    if [[ "$arg" == "pause" ]]; then
        pauseFlag=1
    fi
done

function pause_if_needed() {
    if [[ $pauseFlag -eq 1 ]]; then
        read -p "Press Enter to continue..."
    fi
}

# --------------------------------------------------
echo "1) Create Users (Registration)"
curl -i -X POST http://localhost:10001/users -H "Content-Type: application/json" -d '{"Username":"admin", "Password":"admin12345"}'
echo "Should return HTTP 201"
echo
curl -i -X POST http://localhost:10001/users -H "Content-Type: application/json" -d '{"Username":"kienboec", "Password":"daniel"}'
echo "Should return HTTP 201"
echo
curl -i -X POST http://localhost:10001/users -H "Content-Type: application/json" -d '{"Username":"altenhof", "Password":"markus"}'
echo "Should return HTTP 201"
echo

pause_if_needed

echo "Should fail - User already exists"
curl -i -X POST http://localhost:10001/users -H "Content-Type: application/json" -d '{"Username":"kienboec", "Password":"daniel"}'
echo "Should return HTTP 4xx - User already exists"
echo
curl -i -X POST http://localhost:10001/users -H "Content-Type: application/json" -d '{"Username":"kienboec", "Password":"different"}'
echo "Should return HTTP 4xx - User already exists"
echo

pause_if_needed

# --------------------------------------------------
echo "2) Login Users"
curl -i -X POST http://localhost:10001/sessions -H "Content-Type: application/json" -d '{"Username":"admin", "Password":"admin12345"}'
echo "Should return HTTP 200 with generated token for the user, here: admin-mtcgToken"
echo
curl -i -X POST http://localhost:10001/sessions -H "Content-Type: application/json" -d '{"Username":"kienboec", "Password":"daniel"}'
echo "Should return HTTP 200 with generated token for the user, here: kienboec-mtcgToken"
echo
curl -i -X POST http://localhost:10001/sessions -H "Content-Type: application/json" -d '{"Username":"altenhof", "Password":"markus"}'
echo "Should return HTTP 200 with generated token for the user, here: altenhof-mtcgToken"
echo

pause_if_needed

echo "Should fail - Login with incorrect password"
curl -i -X POST http://localhost:10001/sessions -H "Content-Type: application/json" -d '{"Username":"kienboec", "Password":"wrongpassword"}'
echo "Should return HTTP 4xx - Login failed"
echo

pause_if_needed

# --------------------------------------------------
echo "3) Create Packages (done by 'admin')"
curl -i -X POST http://localhost:10001/packages \
  -H "Authorization: Bearer /pRXkl93BhRQU+TJ9KDHYyKKZ+M1PDI0G5rMA5fHtXk=" \
  -H "Content-Type: application/json" \
  -d '[
        "163f6543-9bd7-4aac-b43e-bf7d60c8cdeb",
        "6b75a18c-747e-49de-a146-c6ae663cc55c",
        "76c23832-9674-48f7-926d-1203bc424f76",
        "4c3aaed8-db9e-493f-b77e-f43d3cf747d0",
        "0a624c0a-9839-454a-8772-fd3a4dffc00e"
      ]'
echo .
curl -i -X POST http://localhost:10001/packages \
  -H "Authorization: Bearer /pRXkl93BhRQU+TJ9KDHYyKKZ+M1PDI0G5rMA5fHtXk=" \
  -H "Content-Type: application/json" \
  -d '[
        "5c719a4a-9fd9-42c2-97c7-1e01ad0dae26",
        "43f430ee-ed93-4d5f-8df1-e9ed3e4f2fd3",
        "2274fbe1-831f-46fe-8f5e-e40523d8dcb6",
        "2a7137e4-5db1-4e9b-b940-526bcf142fb5",
        "d1de297b-ff5b-4edb-a4c5-0a86e45dadd5"
      ]'
echo .
curl -i -X POST http://localhost:10001/packages \
  -H "Authorization: Bearer /pRXkl93BhRQU+TJ9KDHYyKKZ+M1PDI0G5rMA5fHtXk=" \
  -H "Content-Type: application/json" \
  -d '[
        "06f4ba07-7da0-44fb-a4a7-f51b1ff9d3f8",
        "aed238b2-3866-469c-877c-4ffc4e406a40",
        "42c85e7a-cce7-487a-9290-e7ad944b2a17",
        "3e6ad829-cce1-4221-b4f0-302b08626476",
        "70adfd82-c8bd-48f4-bed6-1f56ab9373ad"
      ]'
echo .
curl -i -X POST http://localhost:10001/packages \
  -H "Authorization: Bearer /pRXkl93BhRQU+TJ9KDHYyKKZ+M1PDI0G5rMA5fHtXk=" \
  -H "Content-Type: application/json" \
  -d '[
        "0709dbc3-c730-4fc6-9ded-45f650a5903b",
        "e6d9866c-fd83-46b9-a1ca-028b603fc94c",
        "2d86bf07-015c-4860-982d-f1fe6655d10d",
        "1a6a60d3-a017-40bb-8c38-fcb39b75bcd6",
        "6dab92d1-74e0-4a9e-9a57-dc974138a753"
      ]'
echo .

curl -i -X POST http://localhost:10001/packages \
  -H "Authorization: Bearer /pRXkl93BhRQU+TJ9KDHYyKKZ+M1PDI0G5rMA5fHtXk=" \
  -H "Content-Type: application/json" \
  -d '[
        "6ecc3e51-1839-456f-92db-07121d588adc",
        "13d47dbd-da9a-46bc-9f5d-c2b21008f460",
        "b84f2089-afeb-458c-8893-905fc38bbc81",
        "a259b865-608c-4b65-90ed-6307bbdbe8c7",
        "060bba04-d884-4422-ad1b-9a82090d3d7e"
      ]'
echo .

pause_if_needed

# --------------------------------------------------
echo "4) Acquire Packages (kienboec)"
curl -i -X POST http://localhost:10001/transactions/packages -H "Authorization: Bearer o7neAqf/kzAwJAxfVTFskGMraYjn14bWbqr7VlXG2S8=" -d ""
echo "Should return HTTP 201"
echo
curl -i -X POST http://localhost:10001/transactions/packages -H "Authorization: Bearer o7neAqf/kzAwJAxfVTFskGMraYjn14bWbqr7VlXG2S8=" -d ""
echo "Should return HTTP 201"
echo
curl -i -X POST http://localhost:10001/transactions/packages -H "Authorization: Bearer o7neAqf/kzAwJAxfVTFskGMraYjn14bWbqr7VlXG2S8=" -d ""
echo "Should return HTTP 201"
echo
curl -i -X POST http://localhost:10001/transactions/packages -H "Authorization: Bearer o7neAqf/kzAwJAxfVTFskGMraYjn14bWbqr7VlXG2S8=" -d ""
echo "Should return HTTP 201"
echo
echo "Should fail - Not enough money"
curl -i -X POST http://localhost:10001/transactions/packages -H "Authorization: Bearer o7neAqf/kzAwJAxfVTFskGMraYjn14bWbqr7VlXG2S8=" -d ""
echo "Should return HTTP 4xx - Not enough money"
echo

pause_if_needed

# --------------------------------------------------
echo "5) Acquire Packages (altenhof)"
curl -i -X POST http://localhost:10001/transactions/packages -H "Authorization: Bearer tJBLjDG1+3THiKEMDbZ2ovlogTvqVydoohl1uMu5tIM=" -d ""
echo "Should return HTTP 201"
echo
curl -i -X POST http://localhost:10001/transactions/packages -H "Authorization: Bearer tJBLjDG1+3THiKEMDbZ2ovlogTvqVydoohl1uMu5tIM=" -d ""
echo "Should return HTTP 201"
echo

pause_if_needed

# --------------------------------------------------
echo "6) Show All Cards (kienboec)"
curl -i -X GET http://localhost:10001/cards -H "Authorization: Bearer o7neAqf/kzAwJAxfVTFskGMraYjn14bWbqr7VlXG2S8="
echo "Should return HTTP 200 and all cards"
echo

pause_if_needed

# --------------------------------------------------
echo "7) Configure Deck (kienboec)"
curl -i -X PUT http://localhost:10001/deck -H "Content-Type: application/json" -H "Authorization: Bearer o7neAqf/kzAwJAxfVTFskGMraYjn14bWbqr7VlXG2S8=" -d '[
  "845f0dc7-37d0-426e-994e-43fc3ac83c08",
  "99f8f8dc-e25e-4a95-aa2c-782823f36e2a",
  "e85e3976-7c86-4d06-9a80-641c2019a79f"
]'
echo "Should return HTTP 2xx"
echo

pause_if_needed

# --------------------------------------------------
echo "8) Stats"
curl -i -X GET http://localhost:10001/stats -H "Authorization: Bearer o7neAqf/kzAwJAxfVTFskGMraYjn14bWbqr7VlXG2S8="
echo "Should return HTTP 200 - User stats"
echo

pause_if_needed
# --------------------------------------------------
echo "20) Check Trading Deals"
curl -i -X GET http://localhost:10001/tradings --header "Authorization: Bearer o7neAqf/kzAwJAxfVTFskGMraYjn14bWbqr7VlXG2S8="
echo "Should return HTTP 200 - and an empty list"
echo.

pause_if_needed

echo "21) Create a Trading Deal"
curl -i -X POST http://localhost:10001/tradings --header "Authorization: Bearer o7neAqf/kzAwJAxfVTFskGMraYjn14bWbqr7VlXG2S8=" --header "Content-Type: application/json" -d '{
  "Id": "6cd85277-4590-49d4-b0cf-ba0a921faad0",
  "CardToTrade": "02f87412-d9c2-4c41-99a6-ef51f6fecdab",
  "Type": "monster",
  "MinimumDamage": 15
}'
echo "Should return HTTP 201"
echo.

pause_if_needed

echo "22) Check Trading Deals (After Creating)"
curl -i -X GET http://localhost:10001/tradings --header "Authorization: Bearer o7neAqf/kzAwJAxfVTFskGMraYjn14bWbqr7VlXG2S8="
echo "Should return HTTP 200 - and the trading deal"
echo.
curl -i -X GET http://localhost:10001/tradings --header "Authorization: Bearer tJBLjDG1+3THiKEMDbZ2ovlogTvqVydoohl1uMu5tIM="
echo "Should return HTTP 200 - and the trading deal"
echo.

pause_if_needed

echo "23) Try to Trade with Yourself (Should Fail)"
curl -i -X POST http://localhost:10001/tradings/6cd85277-4590-49d4-b0cf-ba0a921faad0 --header "Authorization: Bearer o7neAqf/kzAwJAxfVTFskGMraYjn14bWbqr7VlXG2S8=" --header "Content-Type: application/json" -d '"38130aa8-b00e-430f-90ca-00e5c5e26066"'
echo "Should return HTTP 4xx"
echo.

pause_if_needed

echo "24) Accept the Trade"
curl -i -X POST http://localhost:10001/tradings/6cd85277-4590-49d4-b0cf-ba0a921faad0 --header "Authorization: Bearer tJBLjDG1+3THiKEMDbZ2ovlogTvqVydoohl1uMu5tIM=" --header "Content-Type: application/json" -d '"38130aa8-b00e-430f-90ca-00e5c5e26066"'
echo "Should return HTTP 201 - Trade Accepted"
echo.

pause_if_needed

echo "25) Check Trading Deals After Trade"
curl -i -X GET http://localhost:10001/tradings --header "Authorization: Bearer o7neAqf/kzAwJAxfVTFskGMraYjn14bWbqr7VlXG2S8="
echo "Should return HTTP 200 - Empty list after trade"
echo.

pause_if_needed

echo "26) Delete Trading Deal"
curl -i -X DELETE http://localhost:10001/tradings/6cd85277-4590-49d4-b0cf-ba0a921faad0 --header "Authorization: Bearer o7neAqf/kzAwJAxfVTFskGMraYjn14bWbqr7VlXG2S8="
echo "Should return HTTP 2xx"
echo.

pause_if_needed
# --------------------------------------------------
echo "End of Script"
