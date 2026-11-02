# Business API approach

1. Define routes, prerequisite gate and sanitized outcome mapping; build Host.
2. Add real loopback HTTP checks with service doubles and isolated test identity injection.
3. Verify malformed/unauthorized/dependency/cancellation cases; document and review contracts.

Keep HTTP types outside Backend. HttpContext-only handlers gate before body binding and
service resolution. No production auth scheme, fake service registration or bypass flag.
Each slice targets 2-5 minutes including relevant checks; split verification as needed.
