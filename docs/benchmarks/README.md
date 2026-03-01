# Benchmark Runs (Manual)

Benchmarks are intentionally **not** executed in CI to keep pipeline cost/time low.

## Prerequisites

- Unity Editor installed locally.
- Optional: set `UNITY_EDITOR_PATH` to a specific `Unity.exe`.

## Commands

From repository root:

- Render benchmark (EditMode tests, `RenderBench` category):
  - `"C:\Path\To\Unity.exe" -batchmode -quit -projectPath "D:\Unity\City Builder TDD" -runTests -testPlatform EditMode -testCategory RenderBench -testResults "D:\Unity\City Builder TDD\artifacts\bench\render-throughput-bench-results.xml" -logFile "D:\Unity\City Builder TDD\artifacts\bench\render-throughput-bench.log"`
- Rail benchmark (EditMode tests, `RailBench` category):
  - `"C:\Path\To\Unity.exe" -batchmode -quit -projectPath "D:\Unity\City Builder TDD" -runTests -testPlatform EditMode -testCategory RailBench -testResults "D:\Unity\City Builder TDD\artifacts\bench\rail-graph-bench-results.xml" -logFile "D:\Unity\City Builder TDD\artifacts\bench\rail-graph-bench.log"`

## Outputs

Reports are generated under:

- `artifacts/bench/render-throughput-bench-results.xml`
- `artifacts/bench/rail-graph-bench-results.xml`

Unity test logs/results (when available) are generated beside reports in `artifacts/bench`.
