using Newtonsoft;
using org.doubango.ultimateAlpr.Sdk;

namespace detector
{
    class Program
    {
        const int YUV_BYTES_PER_PIXEL = 1;

        public struct YUVFrameData
        {
            public byte[] Y { get; set; }
            public byte[] U { get; set; }
            public byte[] V { get; set; }
            public int Width { get; set; }
            public int Height { get; set; }
            public int YStride { get; set; }
            public int UStride { get; set; }
            public int VStride { get; set; }

            public static YUVFrameData Create(
                byte[] Y,
                byte[] U,
                byte[] V,
                int Width,
                int Height,
                int YStride,
                int UStride,
                int VStride
                )
            {
                YUVFrameData frame = new YUVFrameData();
                frame.Y = Y;
                frame.U = U;
                frame.V = V;
                frame.Width = Width;
                frame.Height = Height;
                frame.YStride = YStride;
                frame.UStride = UStride;
                frame.VStride = VStride;
                return frame;
            }
        }

        static void Main(String[] args)
        {
            if (args.Length < 2)
            {
                Console.Error.WriteLine("Incorrect number of arguments. Usage: detector.exe <width> <height>");
                throw new ArgumentException("Incorrect number of arguments. Usage: detector.exe <width> <height>");
            }

            int width, height;

            try
            {
                width = int.Parse(args[0]);
                height = int.Parse(args[1]);
            }
            catch
            {
                Console.Error.WriteLine("Error attempting to parse width and height as integers.");
                throw;
            }

            int ySize = width * height;
            int uvSize = (width / 2) * (height / 2);
            int frameSize = ySize + 2 * uvSize;

            // Initialize YUV buffer
            byte[] buffer = new byte[frameSize];
            int offset = 0;
            int bytesRead = 0;

            // Initialize ALPR
            CheckResult("Init", UltAlprSdkEngine.init(BuildConfigJSON()));

            using (Stream input = Console.OpenStandardInput())
            {
                while ((bytesRead = input.Read(buffer, offset, frameSize - offset)) > 0)
                {
                    offset += bytesRead;

                    if (offset >= frameSize)
                    {
                        byte[] yPlane = new byte[ySize];
                        Array.Copy(buffer, 0, yPlane, 0, ySize);

                        byte[] uPlane = new byte[uvSize];
                        Array.Copy(buffer, ySize, uPlane, 0, uvSize);

                        byte[] vPlane = new byte[uvSize];
                        Array.Copy(buffer, ySize + uvSize, vPlane, 0, uvSize);

                        YUVFrameData frame = YUVFrameData.Create(
                            yPlane,
                            uPlane,
                            vPlane,
                            width,
                            height,
                            width,
                            width / 2,
                            width / 2
                        );

                        offset = 0;
                    }
                }

                if (offset > 0 && offset < frameSize)
                {
                    Console.Error.WriteLine("Incomplete YUV frame detected.");
                }
            }
        }

        static String BuildConfigJSON() => Newtonsoft.Json.JsonConvert.SerializeObject(new
        {
            debug_level = Config.CONFIG_DEBUG_LEVEL,
            debug_write_input_image_enabled = Config.CONFIG_DEBUG_WRITE_INPUT_IMAGE,
            debug_internal_data_path = Config.CONFIG_DEBUG_DEBUG_INTERNAL_DATA_PATH,

            num_threads = Config.CONFIG_NUM_THREADS,
            gpgpu_enabled = Config.CONFIG_GPGPU_ENABLED,
            max_latency = Config.CONFIG_MAX_LATENCY,
            ienv_enabled = Config.CONFIG_IENV_ENABLED,
            openvino_enabled = Config.CONFIG_OPENVINO_ENABLED,
            openvino_device = Config.CONFIG_OPENVINO_DEVICE,

            detect_minscore = Config.CONFIG_DETECT_MINSCORE,
            detect_roi = Config.CONFIG_DETECT_ROI,

            car_noplate_detect_enabled = Config.CONFIG_CAR_NOPLATE_DETECT_ENABLED,
            car_noplate_detect_min_score = Config.CONFIG_CAR_NOPLATE_DETECT_MINSCORE,

            pyramidal_search_enabled = Config.CONFIG_PYRAMIDAL_SEARCH_ENABLED,
            pyramidal_search_sensitivity = Config.CONFIG_PYRAMIDAL_SEARCH_SENSITIVITY,
            pyramidal_search_minscore = Config.CONFIG_PYRAMIDAL_SEARCH_MINSCORE,
            pyramidal_search_min_image_size_inpixels = Config.CONFIG_PYRAMIDAL_SEARCH_MIN_IMAGE_SIZE_INPIXELS,

            klass_lpci_enabled = Config.CONFIG_KLASS_LPCI_ENABLED,
            klass_vcr_enabled = Config.CONFIG_KLASS_VCR_ENABLED,
            klass_vmmr_enabled = Config.CONFIG_KLASS_VMMR_ENABLED,
            klass_vbsr_enabled = Config.CONFIG_KLASS_VBSR_ENABLED,
            klass_vcr_gamma = Config.CONFIG_KLASS_VCR_GAMMA,

            recogn_minscore = Config.CONFIG_RECOGN_MINSCORE,
            recogn_score_type = Config.CONFIG_RECOGN_SCORE_TYPE,
            recogn_rectify_enabled = Config.CONFIG_RECOGN_RECTIFY_ENABLED,

            assets_folder = "B:\\GitRepos\\ultimateALPR-SDK\\assets",
            charset = Config.CONFIG_CHARSET,
            license_token_data = String.Empty,
        });

        static UltAlprSdkResult CheckResult(String functionName, UltAlprSdkResult result)
        {
            if (!result.isOK())
            {
                String errMessage = String.Format("{0}: Execution failed: {1}", new String[] { functionName, result.json() });
                Console.Error.WriteLine(errMessage);
                throw new Exception(errMessage);
            }
            return result;
        }
    }

    public class Config
    {
        /**
        * Defines the debug level to output on the console. You should use "verbose" for diagnostic, "info" in development stage and "warn" on production.
        * JSON name: "debug_level"
        * Default: "info"
        * type: string
        * pattern: "verbose" | "info" | "warn" | "error" | "fatal"
        * More info: https://www.doubango.org/SDKs/anpr/docs/Configuration_options.html#debug-level
        */
        public const String CONFIG_DEBUG_LEVEL = "info";

        /**
         * Whether to write the transformed input image to the disk. This could be useful for debugging.
         * JSON name: "debug_write_input_image_enabled"
         * Default: false
         * type: bool
         * pattern: true | false
         * More info: https://www.doubango.org/SDKs/anpr/docs/Configuration_options.html#debug-write-input-image-enabled
         */
        public const bool CONFIG_DEBUG_WRITE_INPUT_IMAGE = false; // must be false unless you're debugging the code

        /**
         * Path to the folder where to write the transformed input image. Used only if "debug_write_input_image_enabled" is true.
         * JSON name: "debug_internal_data_path"
         * Default: ""
         * type: string
         * pattern: folder path
         * More info: https://www.doubango.org/SDKs/anpr/docs/Configuration_options.html#debug-internal-data-path
         */
        public const String CONFIG_DEBUG_DEBUG_INTERNAL_DATA_PATH = ".";

        /**
         * Defines the maximum number of threads to use.
         * You should not change this value unless you know what you’re doing. Set to -1 to let the SDK choose the right value.
         * The right value the SDK will choose will likely be equal to the number of virtual core.
         * For example, on an octa-core device the maximum number of threads will be 8.
         * JSON name: "num_threads"
         * Default: -1
         * type: int
         * pattern: ]-inf, +inf[
         * More info: https://www.doubango.org/SDKs/anpr/docs/Configuration_options.html#num-threads
         */
        public const int CONFIG_NUM_THREADS = -1;

        /**
         * Whether to enable GPGPU computing. This will enable or disable GPGPU computing on the computer vision and deep learning libraries.
         * On ARM devices this flag will be ignored when fixed-point (integer) math implementation exist for a well-defined function.
         * For example, this function will be disabled for the bilinear scaling as we have a fixed-point SIMD accelerated implementation.
         * Same for many deep learning parts as we’re using QINT8 quantized inference.
         * JSON name: "gpgpu_enabled"
         * Default: true
         * type: bool
         * pattern: true | false
         * More info: https://www.doubango.org/SDKs/anpr/docs/Configuration_options.html#gpgpu-enabled
         */
        public const bool CONFIG_GPGPU_ENABLED = true;

        /**
         * The parallel processing method could introduce delay/latency in the delivery callback on low-end CPUs. 
         * This parameter controls the maximum latency you can tolerate. The unit is number of frames. 
         * The default value is -1 which means auto.
         * JSON name: "max_latency"
         * Default: -1
         * type: int
         * pattern: [0, +inf[
         * More info: https://www.doubango.org/SDKs/anpr/docs/Configuration_options.html#max-latency
         */
        public const int CONFIG_MAX_LATENCY = -1;

        /**
         * Defines a charset (Alphabet) to use for the recognizer.
         * JSON name: "charset"
         * Default: "latin"
         * type: string
         * pattern: "latin" | "korean" | "chinese"
         * More info: https://www.doubango.org/SDKs/anpr/docs/Configuration_options.html#charset
         */
        public const String CONFIG_CHARSET = "latin";

        /**
         * Whether to enable Image Enhancement for Night-Vision (IENV).
         * IENV is explained at https://www.doubango.org/SDKs/anpr/docs/Features.html#features-imageenhancementfornightvision.
         *
         * JSON name: "ienv_enabled"
         * Default: false
         * type: bool
         * pattern: true | false
         * Available since: 3.2.0
         * More info: https://www.doubango.org/SDKs/anpr/docs/Configuration_options.html#ienv-enabled
         */
        public const bool CONFIG_IENV_ENABLED = false;

        /**
         * Whether to use OpenVINO instead of Tensorflow as deep learning backend engine. OpenVINO is used for detection and classification but not for OCR. 
         * OpenVINO is always faster than Tensorflow on Intel products (CPUs, VPUs, GPUs, FPGAs…) and we highly recommend using it. 
         * We require a CPU with support for both AVX2 and FMA features before trying to load OpenVINO plugin (shared library). 
         * OpenVINO will be disabled with a fallback on Tensorflow if these CPU features are not detected.
         * JSON name: "openvino_enabled"
         * Default: true
         * type: bool
         * pattern: true | false
         * Available since: 3.0.0
         * More info: https://www.doubango.org/SDKs/anpr/docs/Configuration_options.html#openvino-enabled
         */
        public const bool CONFIG_OPENVINO_ENABLED = true;

        /**
         * OpenVINO device to use for computations. We recommend using "CPU" which is always correct. 
         * If you have an Intel GPU, VPU or FPGA, then you can change this value. 
         * If you try to use any other value than "CPU" without having the right device, then OpenVINO will be completely disabled with a fallback on Tensorflow. 
         * JSON name: "openvino_device"
         * Default: "CPU"
         * type: string
         * pattern: "GNA" | "HETERO" | "CPU" | "MULTI" | "GPU" | "MYRIAD" | "HDDL " | "FPGA"
         * Available since: 3.0.0
         * More info: https://www.doubango.org/SDKs/anpr/docs/Configuration_options.html#openvino-device
         */
        public const String CONFIG_OPENVINO_DEVICE = "CPU";

        /**
         * Define a threshold for the detection score. Any detection with a score below that threshold will be ignored. 0.f being poor confidence and 1.f excellent confidence.
         * JSON name: "detect_minscore",
         * Default: 0.3f
         * type: float
         * pattern: ]0.f, 1.f]
         * More info: https://www.doubango.org/SDKs/anpr/docs/Configuration_options.html#detect-minscore
         */
        public const double CONFIG_DETECT_MINSCORE = 0.3;

        /**
         * Defines the Region Of Interest (ROI) for the detector. Any pixels outside region of interest will be ignored by the detector.
         * Defining an WxH region of interest instead of resizing the image at WxH is very important as you'll keep the same quality when you define a ROI while you'll lose in quality when using the later.
         * JSON name: "detect_roi"
         * Default: [0.f, 0.f, 0.f, 0.f]
         * type: float[4]
         * pattern: [left, right, top, bottom]
         * More info: https://www.doubango.org/SDKs/anpr/docs/Configuration_options.html#detect-roi
         */
        public static readonly IList<float> CONFIG_DETECT_ROI = new[] { 0f, 0f, 0f, 0f };

        /**
         * Whether to return cars with no plate. By default any car without plate will be silently ignored.
         * To filter false-positives: https://www.doubango.org/SDKs/anpr/docs/Known_issues.html#false-positives-for-cars-with-no-plate
         * JSON name: "car_noplate_detect_enabled"
         * Default: false
         * type: bool
         * pattern: true | false
         * Available since: 3.2.0
         * More info: https://www.doubango.org/SDKs/anpr/docs/Configuration_options.html#car-noplate-detect-enabled
         */
        public const bool CONFIG_CAR_NOPLATE_DETECT_ENABLED = false;

        /**
        * Defines a threshold for the detection score for cars with no plate. Any detection with a score below that threshold will be ignored. 0.f being poor confidence and 1.f excellent confidence.
        * JSON name: "car_noplate_detect_min_score",
        * Default: 0.8f
        * type: float
        * pattern: [0.f, 1.f]
        * Available since: 3.2.0
        * More info: https://www.doubango.org/SDKs/anpr/docs/Configuration_options.html#car-noplate-detect-min-score
        */
        public const double CONFIG_CAR_NOPLATE_DETECT_MINSCORE = 0.8; // 80%

        /**
         * Whether to enable pyramidal search. Pyramidal search is an advanced feature to accurately detect very small or far away license plates.
         * JSON name: "pyramidal_search_enabled"
         * Default: true
         * type: bool
         * pattern: true | false
         * More info: https://www.doubango.org/SDKs/anpr/docs/Configuration_options.html#pyramidal-search-enabled
         */
        public const bool CONFIG_PYRAMIDAL_SEARCH_ENABLED = true;

        /**
         * Defines how sensitive the pyramidal search anchor resolution function should be. The higher this value is, the higher the number of pyramid levels will be.
         * More levels means better accuracy but higher CPU usage and inference time.
         * Pyramidal search will be disabled if this value is equal to 0.
         * JSON name: "pyramidal_search_sensitivity"
         * Default: 0.28f
         * type: float
         * pattern: [0.f, 1.f]
         * More info: https://www.doubango.org/SDKs/anpr/docs/Configuration_options.html#pyramidal-search-sensitivity
         */
        public const double CONFIG_PYRAMIDAL_SEARCH_SENSITIVITY = 0.33; // 33%

        /**
         * Defines a threshold for the detection score associated to the plates retrieved after pyramidal search.
         * Any detection with a score below that threshold will be ignored.
         * 0.f being poor confidence and 1.f excellent confidence.
         * JSON name: "pyramidal_search_minscore"
         * Default: 0.8f
         * type: float
         * pattern: ]0.f, 1.f]
         * More info: https://www.doubango.org/SDKs/anpr/docs/Configuration_options.html#pyramidal-search-minscore
         */
        public const double CONFIG_PYRAMIDAL_SEARCH_MINSCORE = 0.3; // 30%

        /**
         * Minimum image size (max[width, height]) in pixels to trigger pyramidal search.
         * Pyramidal search will be disabled if the image size is less than this value. Using pyramidal search on small images is useless.
         * JSON name: "pyramidal_search_min_image_size_inpixels"
         * Default: 800
         * type: integer
         * pattern: [0, inf]
         * More info: https://www.doubango.org/SDKs/anpr/docs/Configuration_options.html#pyramidal-search-min-image-size-inpixels
         */
        public const int CONFIG_PYRAMIDAL_SEARCH_MIN_IMAGE_SIZE_INPIXELS = 800; // pixels

        /**
         * Whether to enable License Plate Country Identification (LPCI) function (https://www.doubango.org/SDKs/anpr/docs/Features.html#license-plate-country-identification-lpci). 
         * To avoid adding latency to the pipeline only enable this function if you really need it.
         * JSON name: "klass_lpci_enabled"
         * Default: false
         * type: bool
         * pattern: true | false
         * Available since: 3.0.0
         * More info at https://www.doubango.org/SDKs/anpr/docs/Configuration_options.html#klass-lpci-enabled
         */
        public const bool CONFIG_KLASS_LPCI_ENABLED = false;

        /**
         * Whether to enable Vehicle Color Recognition (VCR) function (https://www.doubango.org/SDKs/anpr/docs/Features.html#vehicle-color-recognition-vcr). 
         * To avoid adding latency to the pipeline only enable this function if you really need it.
         * JSON name: "klass_vcr_enabled"
         * Default: false
         * type: bool
         * pattern: true | false
         * Available since: 3.0.0
         * More info at https://www.doubango.org/SDKs/anpr/docs/Configuration_options.html#klass-vcr-enabled
         */
        public const bool CONFIG_KLASS_VCR_ENABLED = false;

        /**
         * Whether to enable Vehicle Make Model Recognition (VMMR) function (https://www.doubango.org/SDKs/anpr/docs/Features.html#vehicle-make-model-recognition-vmmr).
         * To avoid adding latency to the pipeline only enable this function if you really need it.
         * JSON name: "klass_vmmr_enabled"
         * Default: false
         * type: bool
         * pattern: true | false
         * Available since: 3.0.0
         * More info at https://www.doubango.org/SDKs/anpr/docs/Configuration_options.html#klass-vmmr-enabled
         */
        public const bool CONFIG_KLASS_VMMR_ENABLED = false;

        /**
         * Whether to enable Vehicle Body Style Recognition (VBSR) function (https://www.doubango.org/SDKs/anpr/docs/Features.html#features-vehiclebodystylerecognition).
         * To avoid adding latency to the pipeline only enable this function if you really need it.
         * JSON name: "klass_vbsr_enabled"
         * Default: false
         * type: bool
         * pattern: true | false
         * Available since: 3.2.0
         * More info at https://www.doubango.org/SDKs/anpr/docs/Configuration_options.html#klass-vbsr-enabled
         */
        public const bool CONFIG_KLASS_VBSR_ENABLED = false;

        /**
         * 1/G coefficient value to use for gamma correction operation in order to enhance the car color before applying VCR classification. 
         * More information on gamma correction could be found at https://en.wikipedia.org/wiki/Gamma_correction. 
         * Values higher than 1.0f mean lighter and lower than 1.0f mean darker. Value equal to 1.0f mean bypass gamma correction operation.
         * This parameter in action: https://www.doubango.org/SDKs/anpr/docs/Improving_the_accuracy.html#gamma-correction
         * * JSON name: "recogn_minscore"
         * Default: 1.5
         * type: float
         * pattern: [0.f, inf[
         * Available since: 3.0.0
         * More info: https://www.doubango.org/SDKs/anpr/docs/Configuration_options.html#klass-vcr-gamma
         */
        public const double CONFIG_KLASS_VCR_GAMMA = 1.5;

        /**
         * Define a threshold for the overall recognition score. Any recognition with a score below that threshold will be ignored.
         * The overall score is computed based on "recogn_score_type". 0.f being poor confidence and 1.f excellent confidence.
         * JSON name: "recogn_minscore"
         * Default: 0.3f
         * type: float
         * pattern: ]0.f, 1.f]
         * More info: https://www.doubango.org/SDKs/anpr/docs/Configuration_options.html#recogn-minscore
         */
        public const double CONFIG_RECOGN_MINSCORE = 0.2; // 20%

        /**
         * Defines the overall score type. The recognizer outputs a recognition score ([0.f, 1.f]) for every character in the license plate.
         * The score type defines how to compute the overall score.
         * - "min": Takes the minimum score.
         * - "mean": Takes the average score.
         * - "median": Takes the median score.
         * - "max": Takes the maximum score.
         * - "minmax": Takes (max + min) * 0.5f.
         * The "min" score is the more robust type as it ensure that every character have at least a certain confidence value.
         * The median score is the default type as it provide a higher recall. In production we recommend using min type.
         * JSON name: "recogn_score_type"
         * Default: "median"
         * Recommended: "min"
         * type: string
         *  More info: https://www.doubango.org/SDKs/anpr/docs/Configuration_options.html#recogn-score-type
         */
        public const String CONFIG_RECOGN_SCORE_TYPE = "min";

        /**
         * Whether to add rectification layer between the detector’s output and the recognizer’s input. A rectification layer is used to suppress the distortion.
         * A plate is distorted when it’s skewed and/or slanted. The rectification layer will deslant and deskew the plate to make it straight which make the recognition more accurate.
         * Please note that you only need to enable this feature when the license plates are highly distorted. The implementation can handle moderate distortion without a rectification layer.
         * The rectification layer adds many CPU intensive operations to the pipeline which decrease the frame rate.
         * More info on the rectification layer could be found at https://www.doubango.org/SDKs/anpr/docs/Rectification_layer.html#rectificationlayer
         * JSON name: "recogn_rectify_enabled"
         * Default: false
         * Recommended: false
         * type: string
         * More info at https://www.doubango.org/SDKs/anpr/docs/Configuration_options.html#recogn-rectify-enabled
         */
        public const bool CONFIG_RECOGN_RECTIFY_ENABLED = true;
    }
}
