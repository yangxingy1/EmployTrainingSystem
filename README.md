# 慧动手-在线数据系统

## 前端重构指令：

```
cd frontend
npm ci
npm run build

sudo rm -rf /var/www/employ-training/*
sudo cp -r /dist/* /var/www/employ-training/
```

## 后端运行指令：

```
cd EmployTrainingSystem

uvicorn backend.main:app --host 127.0.0.1 --port 8000
```

