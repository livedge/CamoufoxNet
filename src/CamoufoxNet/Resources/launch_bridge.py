"""Bridge script called by CamoufoxNet to generate Camoufox launch options."""
import sys, json, os, io


def main():
    # Read options JSON from stdin
    options = json.loads(sys.stdin.read())

    # Redirect stdout to stderr during import/call to prevent
    # library output (e.g. "Downloading model files...") from corrupting JSON
    real_stdout = sys.stdout
    sys.stdout = sys.stderr

    # Import camoufox
    from camoufox.utils import launch_options as _launch_options

    # Map C# options to Python kwargs
    kwargs = {}
    if options.get("os"):
        kwargs["os"] = options["os"]
    if options.get("headless") is not None:
        kwargs["headless"] = options["headless"]
    if options.get("humanize"):
        kwargs["humanize"] = options["humanize"]
    if options.get("geoip"):
        kwargs["geoip"] = options["geoip"]
    if options.get("locale"):
        kwargs["locale"] = options["locale"]
    if options.get("block_images"):
        kwargs["block_images"] = True
    if options.get("block_webrtc"):
        kwargs["block_webrtc"] = True
    if options.get("block_webgl"):
        kwargs["block_webgl"] = True
    if options.get("disable_coop"):
        kwargs["disable_coop"] = True
    if options.get("screen"):
        from browserforge.fingerprints import Screen
        s = options["screen"]
        kwargs["screen"] = Screen(**s)
    if options.get("proxy"):
        kwargs["proxy"] = options["proxy"]
    if options.get("fonts"):
        kwargs["fonts"] = options["fonts"]
    if options.get("addons"):
        kwargs["addons"] = options["addons"]
    if options.get("window"):
        kwargs["window"] = tuple(options["window"])
    if options.get("webgl_config"):
        kwargs["webgl_config"] = tuple(options["webgl_config"])
    if options.get("config"):
        kwargs["config"] = options["config"]
    if options.get("firefox_user_prefs"):
        kwargs["firefox_user_prefs"] = options["firefox_user_prefs"]
    if options.get("executable_path"):
        kwargs["executable_path"] = options["executable_path"]
    if options.get("enable_cache"):
        kwargs["enable_cache"] = True
    if options.get("args"):
        kwargs["args"] = options["args"]
    if options.get("env"):
        kwargs["env"] = options["env"]
    if options.get("main_world_eval"):
        kwargs["main_world_eval"] = True
    if options.get("ff_version"):
        kwargs["ff_version"] = options["ff_version"]
    if options.get("exclude_addons"):
        kwargs["exclude_addons"] = options["exclude_addons"]

    # Call camoufox launch_options
    result = _launch_options(**kwargs)

    # Restore stdout for JSON output
    sys.stdout = real_stdout

    # Serialize for C#
    output = {}
    output["executable_path"] = result.get("executable_path", "")
    output["headless"] = result.get("headless", False)
    output["args"] = list(result.get("args", []))

    # env dict: convert all values to strings
    env = result.get("env", {})
    output["env"] = {k: str(v) for k, v in env.items()}

    # firefox_user_prefs: keep as-is (mixed types)
    output["firefox_user_prefs"] = result.get("firefox_user_prefs", {})

    # proxy
    if result.get("proxy"):
        output["proxy"] = result["proxy"]

    # Any extra Playwright options
    for key in result:
        if key not in output and result[key] is not None:
            try:
                json.dumps(result[key])  # only include serializable values
                output[key] = result[key]
            except (TypeError, ValueError):
                pass

    json.dump(output, sys.stdout)


if __name__ == "__main__":
    main()
