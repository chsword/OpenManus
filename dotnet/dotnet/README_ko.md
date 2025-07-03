# OpenManus .NET 프로젝트

이 프로젝트는 다양한 도구를 사용하여 여러 작업을 해결할 수 있는 다목적 에이전트를 구현합니다. 이 문서는 프로젝트의 구조와 사용 방법에 대한 정보를 제공합니다.

## 프로젝트 구조

- `src/OpenManus.Core`: 핵심 라이브러리
  - `Models`: 데이터 모델 정의
  - `Exceptions`: 사용자 정의 예외 클래스
  - `Configuration`: 구성 서비스
  - `Logging`: 로깅 서비스
- `src/OpenManus.Agent`: 에이전트 서비스
- `src/OpenManus.Flow`: 흐름 서비스
- `src/OpenManus.Llm`: LLM 서비스
- `src/OpenManus.Mcp`: MCP 서비스
- `src/OpenManus.Tools`: 도구 서비스
- `src/OpenManus.Sandbox`: 샌드박스 서비스
- `src/OpenManus.Prompt`: 프롬프트 서비스
- `src/OpenManus.Console`: 콘솔 애플리케이션

## 설치 및 실행

1. .NET SDK 9.x를 설치합니다.
2. 프로젝트를 클론합니다.
3. `src/OpenManus.sln` 파일을 열고 모든 프로젝트를 빌드합니다.
4. 콘솔 애플리케이션을 실행하여 기능을 테스트합니다.

## 기여

기여를 원하시는 분은 이 저장소를 포크하고 변경 사항을 제안해 주세요. 모든 기여는 환영합니다!

## 라이센스

이 프로젝트는 MIT 라이센스 하에 배포됩니다. 자세한 내용은 `LICENSE` 파일을 참조하세요.