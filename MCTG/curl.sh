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
  -H "Authorization: Bearer XxImjATrx/4XAB4sZuXeDCKtUB0Uglye1yYF/KifzBs=" \
  -H "Content-Type: application/json" \
  -d '[
        "02f87412-d9c2-4c41-99a6-ef51f6fecdab",
        "02f87412-d9c2-4c41-99a6-ef51f6fecdab",
        "fc31d1f3-f90d-48c0-af48-f08aa8d3b3c6",
        "fc31d1f3-f90d-48c0-af48-f08aa8d3b3c6",
        "38130aa8-b00e-430f-90ca-00e5c5e26066"
      ]'
echo "Should return HTTP 201"
echo
curl -i -X POST http://localhost:10001/packages \
  -H "Authorization: Bearer XxImjATrx/4XAB4sZuXeDCKtUB0Uglye1yYF/KifzBs=" \
  -H "Content-Type: application/json" \
  -d '[
        "02f87412-d9c2-4c41-99a6-ef51f6fecdab",
        "02f87412-d9c2-4c41-99a6-ef51f6fecdab",
        "fc31d1f3-f90d-48c0-af48-f08aa8d3b3c6",
        "fc31d1f3-f90d-48c0-af48-f08aa8d3b3c6",
        "38130aa8-b00e-430f-90ca-00e5c5e26066"
      ]'
echo "Should return HTTP 201"
echo
curl -i -X POST http://localhost:10001/packages \
  -H "Authorization: Bearer XxImjATrx/4XAB4sZuXeDCKtUB0Uglye1yYF/KifzBs=" \
  -H "Content-Type: application/json" \
  -d '[
        "02f87412-d9c2-4c41-99a6-ef51f6fecdab",
        "02f87412-d9c2-4c41-99a6-ef51f6fecdab",
        "fc31d1f3-f90d-48c0-af48-f08aa8d3b3c6",
        "fc31d1f3-f90d-48c0-af48-f08aa8d3b3c6",
        "38130aa8-b00e-430f-90ca-00e5c5e26066"
      ]'
echo "Should return HTTP 201"
echo
curl -i -X POST http://localhost:10001/packages \
  -H "Authorization: Bearer XxImjATrx/4XAB4sZuXeDCKtUB0Uglye1yYF/KifzBs=" \
  -H "Content-Type: application/json" \
  -d '[
        "02f87412-d9c2-4c41-99a6-ef51f6fecdab",
        "02f87412-d9c2-4c41-99a6-ef51f6fecdab",
        "fc31d1f3-f90d-48c0-af48-f08aa8d3b3c6",
        "fc31d1f3-f90d-48c0-af48-f08aa8d3b3c6",
        "38130aa8-b00e-430f-90ca-00e5c5e26066"
      ]'
echo "Should return HTTP 201"
echo
curl -i -X POST http://localhost:10001/packages \
  -H "Authorization: Bearer XxImjATrx/4XAB4sZuXeDCKtUB0Uglye1yYF/KifzBs=" \
  -H "Content-Type: application/json" \
  -d '[
        "02f87412-d9c2-4c41-99a6-ef51f6fecdab",
        "02f87412-d9c2-4c41-99a6-ef51f6fecdab",
        "fc31d1f3-f90d-48c0-af48-f08aa8d3b3c6",
        "fc31d1f3-f90d-48c0-af48-f08aa8d3b3c6",
        "38130aa8-b00e-430f-90ca-00e5c5e26066"
      ]'
echo "Should return HTTP 201"
echo

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
echo "End of Script"
