# 002: Basic local review frontend

Status: Pending user review (DEC-042).

Use browser JavaScript modules and CSS, served by Node's built-in HTTP server, for
the basic seeded-document exercise. Alternative: a framework and build pipeline.
The small current surface does not require additional packages. A future framework
can retain the isolated API/state module.

Data flow: browser -> local Node fixed-origin API proxy -> C# read/save endpoints ->
backend persistence. The proxy includes no identity bypass or fake persistence.
The separate sample is explicitly non-persistent. No upload or translation UI exists.
