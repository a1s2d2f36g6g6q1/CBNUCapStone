# PUZZLE 22

### 프로젝트 설명

본 프로젝트는 사용자가 입력한 태그 기반으로 AI가 생성한 이미지를 슬라이딩 퍼즐 형태로 변환하여 사용자에게 다양한 시각적 콘텐츠를 제공한다.
사용자마다 하나의 행성을 소유하여 행성 내에 퍼즐을 통해 얻은 이미지를 저장한다. 이를 다른 사용자와 공유 및 상호작용을 경험하게 된다.

### 개발기간

-   2024.09. ~ 2025.11.

### 기여자

| 서동우 | 이은총 | 최유림 |
| ------ | ------ | ------ |
|        |        |        |

### 기술 스택

-   Frontend: Unity(C#)
-   Backend: Node.js(Express), MySQL, AWS

### 서비스설명

아래와 같은 기능을 가지고 있는 웹 서비스 입니다.

1. 싱글플레이 기능

    - 키워드 4개와 난이도를 선택하여 AI 이미지 기반으로 퍼즐을 생성하고, 클리어할 경우 행성에 저장할 수 있습니다.  
      <img src="./image/image-2.png" alt="설명" width="400" />  
      <img src="./image/image-3.png" alt="설명" width="400" />  
      <img src="./image/image-0.png" alt="설명" width="400" />

1. 멀티플레이 기능

    - 팀장의 경우 방을 생성하여 코드를 반환받고, 이 코드를 통해 팀원은 해당 방에 참여하여 최대 4명으로 멀티플레이가 가능합니다.  
      <img src="./image/image.png" alt="설명" width="400" />  
      <img src="./image/image-1.png" alt="설명" width="400" />

1. 행성 탐색 기능
    - 싱글 또는 멀티플레이에서 얻은 AI 이미지를 행성에 저장하여 다른 사용자와 공유를 할 수 있습니다. 또한 방명록을 남길 수도 있습니다.  
      <img src="./image/image-5.png" alt="설명" width="400" />
1. 친구 관리 기능
    - 친구 신청, 수락, 삭제 등의 기능입니다.
      <img src="./image/image-4.png" alt="설명" width="400" />

### 설치 및 실행 방법

#### 일반 사용자 (게임 플레이)

1. [Releases](https://github.com/a1s2d2f36g6g6q1/CBNUCapStone/releases) 페이지에서 최신 버전의 실행 파일을 다운로드합니다.
2. 압축을 해제하고 `PUZZLE22.exe` 파일을 실행합니다.
3. 첫 실행 시 Windows Defender 경고가 나타날 수 있습니다. "추가 정보" → "실행"을 클릭하여 진행합니다.

#### 개발자 (프로젝트 빌드)

1. Unity Hub 설치: [Unity 공식 사이트](https://unity.com/download)
2. Unity Editor 2022.3.22f1 설치
    - Unity Hub → Installs → Install Editor → Archive에서 2022.3.22f1 버전 선택
    - WebGL Build Support 모듈 포함하여 설치
3. 저장소 클론:

```bash
   git clone https://github.com/사용자명/저장소명.git
```

4. Unity Hub에서 "Add" → 클론한 프로젝트 폴더 선택
5. 프로젝트 열기 및 필요한 패키지 자동 설치 대기
6. Asset Store 에셋 수동 설치:
    - [Dark Theme UI](https://assetstore.unity.com/packages/2d/gui/dark-theme-ui-199010)
    - [Sleek Essential UI Pack](https://assetstore.unity.com/packages/2d/gui/icons/sleek-essential-ui-pack-170650)
7. NuGet for Unity를 통해 SocketIOClient 설치

### 의존성

이 프로젝트는 다음과 같은 의존성을 가집니다.

#### 백엔드

```
@aws-sdk/client-s3  ^3.906.0
axios               ^1.12.2
bcrypt              ^6.0.0
dotenv              ^17.2.3
express             ^5.1.0
jsonwebtoken        ^9.0.2
mysql2              ^3.15.1
react               ^19.1.1
socket.io           ^4.8.1
socket.io-client    ^4.8.1
uuid                ^13.0.0

```

#### 프론트엔드

```
Unity Version: 2022.3.22f1 LTS

Unity Modules:
- WebGL Build Support

Unity Packages:
- TextMeshPro

Asset Store Assets:
- Dark Theme UI (https://assetstore.unity.com/packages/2d/gui/dark-theme-ui-199010)
- Sleek Essential UI Pack (https://assetstore.unity.com/packages/2d/gui/icons/sleek-essential-ui-pack-170650)

NuGet Packages:
- SocketIOClient
```

### License

```
MIT License

Copyright (c) 2024 cbnuBooklog

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
```
