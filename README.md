# ToyParty (과제 프로젝트)

## 개요
쿡앱스에서 서비스중인 게임 Toy Party - Hexa Blast 게임에서 21레벨 스테이지 구현 과제.
플레이어는 블럭을 드래그하여 스왑할 수 있으며, 같은 색 블럭이 3개 이상 매칭되면 제거되고,  
중력에 의해 위 블럭이 떨어지며 새로운 블럭이 스폰됩니다.  
특수 블럭(레인보우, 로켓) 생성 및 발동, 팽이(장애물) 처리까지 구현되어 있습니다.

## 프로젝트
- **Unity 6000.0.43f1**

## 주요 기능
- **보드 & 타일**
  - `Tile`을 기반으로 보드 셀 구성
  - `LevelData`를 통해 보드 크기, 스폰 포인트, 초기 배치 설정 가능

- **블럭 시스템 (`Block` 상속 구조)**
  - `GemBlock` : 일반 매치 가능한 보석 블럭
  - `RocketBlock` : 4개 매치 → 라인 제거
  - `RainbowBlock` : 5개 매치 → 특정 색 전체 제거
  - `SpinningBlock` : 색이 없는 장애물, 주변 매치/특수 발동 시 제거
  - 
- **매치 & 루프 (`MatchManager`)**
  - 매치 탐색 → 제거 → 중력 낙하 → 스폰 루프
  - 직하 우선 → 대각 낙하 보조
  - 스왑 후 매치가 없으면 원위치 복귀
  - 특수 블럭 생성/발동 로직 포함

- **입력 (`InputController`)**
  - 마우스 드래그/터치 드래그로 블럭 스왑
  - 레인보우 블럭 스왑 시 대상 색 전체 제거

## 실행 방법
1. Unity 프로젝트 열기
2. `MatchScene` 실행
3. 마우스로 블럭을 드래그하여 매치-3 게임 플레이

![3_Match](https://github.com/user-attachments/assets/626f77cf-4d0b-4e1a-9a88-c8dc6c855a75)

![Spinning](https://github.com/user-attachments/assets/c20932dc-7d49-4f49-855b-cc15629c6fea)

![Rainbow](https://github.com/user-attachments/assets/b583f4fa-37a3-48b0-8c53-92483e7cfd18)

![Rocket](https://github.com/user-attachments/assets/5c6097e6-4dc7-493d-b422-40042920b632)
