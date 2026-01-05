# HospitalTriage UI Tests (Selenium + TestNG)

This is a Maven test project to run UI automation against the HospitalTriageSystem web app.

## 1) Prerequisites

- JDK **17**
- Maven **3.8+**
- Chrome / Edge / Firefox installed
- The web app must be running locally at:
  - `https://localhost:64274/` (default)

> The tests are configured to **accept insecure localhost HTTPS certificates**.

## 2) Open in Eclipse (Maven)

1. **File → Import… → Maven → Existing Maven Projects**
2. Select the folder containing this `pom.xml`.
3. Ensure Eclipse is using **JDK 17**:
   - Window → Preferences → Java → Installed JREs
4. Right‑click project → Maven → **Update Project…**

## 3) Configure (baseUrl / browser / headless / accounts)

Edit `src/test/resources/config.properties`.

Key options:

- `baseUrl=https://localhost:64274/`
- `browser=chrome | firefox | edge`
- `headless=true | false`

Accounts are seeded by your ASP.NET project (`DbInitializer`).

## 4) Run tests

### Option A: Maven CLI

```bash
mvn clean test
```

Override config at runtime:

```bash
mvn clean test -DbaseUrl=https://localhost:64274 -Dbrowser=chrome -Dheadless=true
```

### Option B: Eclipse

- Right click project → **Run As → Maven test**
- Or run `src/test/resources/testng.xml` as a TestNG suite.

## 5) Where to read reports + failure artifacts

After `mvn test`, open:

- **Execution log** (readable, includes URL + artifact paths):
  - `target/test-output/execution.log`

- **Screenshots on failure**:
  - `target/test-output/screenshots/*.png`

- **Page source on failure** (helps debug selectors / validation):
  - `target/test-output/pagesource/*.html`

- **Surefire reports** (per-test stdout/stderr, XML, etc.):
  - `target/test-output/surefire-reports/`

## Notes

- UI tests don't directly measure **.NET server code coverage**.
  If you need real server-side coverage, combine:
  - .NET unit/integration tests + Coverlet
  - UI tests for end-to-end flows.
