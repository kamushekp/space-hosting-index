#!/bin/bash -e
THIS_SCRIPT_DIR=$(cd $(dirname "${BASH_SOURCE[0]}") && pwd)

if [ $# -lt 1 ]; then
    echo "Usage: .build-faiss-debian.sh faiss_version";
    exit 1;
fi

FAISS_VERSION=$1
DOCKER_IMAGE_NAME_AND_TAG=space-hosting/faiss-lib:$FAISS_VERSION

echo "Building faiss-lib docker image: $DOCKER_IMAGE_NAME_AND_TAG"

docker image build \
    --pull \
    --build-arg "FAISS_VERSION=$FAISS_VERSION" \
    --tag "$DOCKER_IMAGE_NAME_AND_TAG" \
    --file "$THIS_SCRIPT_DIR/.build-faiss-debian.dockerfile" \
    "$THIS_SCRIPT_DIR"
