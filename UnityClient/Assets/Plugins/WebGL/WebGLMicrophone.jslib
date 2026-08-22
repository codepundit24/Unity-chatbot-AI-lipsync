mergeInto(LibraryManager.library, {

    StartWebGLRecording: function () {

        if (!navigator.mediaDevices || !navigator.mediaDevices.getUserMedia) {
            console.error("Microphone API not supported");
            return;
        }

        navigator.mediaDevices.getUserMedia({ audio: true })
            .then(function (stream) {

                window.webglMicStream = stream;
                window.webglAudioChunks = [];

                window.webglMediaRecorder = new MediaRecorder(stream);

                window.webglMediaRecorder.ondataavailable = function (event) {
                    if (event.data.size > 0) {
                        window.webglAudioChunks.push(event.data);
                    }
                };

                window.webglMediaRecorder.onstop = function () {

                    var blob = new Blob(
                        window.webglAudioChunks,
                        { type: "audio/webm" }
                    );

                    var reader = new FileReader();

                    reader.onloadend = function () {

                        // Remove "data:audio/webm;base64,"
                        var base64data = reader.result.split(",")[1];

                        // Send recording back to Unity
                        SendMessage(
                            "Chatbot",
                            "OnWebGLRecordingReady",
                            base64data
                        );
                    };

                    reader.readAsDataURL(blob);

                    if (window.webglMicStream) {
                        window.webglMicStream.getTracks().forEach(function(track) {
                            track.stop();
                        });
                    }
                };

                window.webglMediaRecorder.start();

                console.log("WebGL microphone recording started");
            })
            .catch(function (error) {
                console.error("Microphone permission/error: ", error);
            });
    },

    StopWebGLRecording: function () {

        if (
            window.webglMediaRecorder &&
            window.webglMediaRecorder.state !== "inactive"
        ) {
            window.webglMediaRecorder.stop();
            console.log("WebGL microphone recording stopped");
        }
    }

});