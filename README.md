# hd-speed-test

.NET Windows application to test end-to-end read and write performance of a storage device.

## Usage
`hd-speed-test.exe <TESTPATH> [--human]`

Without any flags, results are newline JSON, with milliseconds and bytes-per-milliseconds as units. With the `--human` flag, units are in seconds and megabytes per second.
